namespace StoreManagement.Application.Features.Invoices.Commands;

public class CreateSalesInvoiceCommand : IRequest<long>
{
    public long CustomerId { get; set; }
    public string InvoiceNumber { get; set; }
    public DateTime InvoiceDate { get; set; }
    public PaymentType PaymentType { get; set; }
    public long? BankAccountId { get; set; }
    public InstallmentDetailsDto? InstallmentDetails { get; set; }
    public List<InvoiceItemDto> Items { get; set; }
}


public class CreateSalesInvoiceCommandHandler(
    IGenericRepository<SalesInvoice> invoiceRepository,
    IGenericRepository<InventoryTransaction> inventoryRepository,
    IGenericRepository<ProductVariant> variantRepository,
    IGenericRepository<Installment> installmentRepository,
    IGenericRepository<BankAccount> bankAccountRepository,
    ILogger<CreatePurchaseInvoiceCommandHandler> logger,
    IDapperRepository dapperRepository)
    : IRequestHandler<CreateSalesInvoiceCommand, long>
{
    public async Task<long> Handle(CreateSalesInvoiceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            foreach (var itemDto in request.Items)
            {
                var stockCheckSql = @"
                    SELECT COALESCE(SUM(CASE WHEN it.TransactionTypeId = 1 THEN it.Quantity WHEN it.TransactionTypeId = 2 THEN -it.Quantity ELSE 0 END), 0) as CurrentStock
                    FROM InventoryTransactions it
                    WHERE it.ProductVariantId = @ProductVariantId";
                var currentStock = await dapperRepository.QuerySingleOrDefaultAsync<int>(stockCheckSql, new { ProductVariantId = itemDto.ProductVariantId });

                if (currentStock < itemDto.Quantity)
                {
                    throw new InvalidOperationException($"موجودی کالای با شناسه {itemDto.ProductVariantId} کافی نیست. موجودی فعلی: {currentStock}");
                }
            }

            var salesInvoice = new SalesInvoice(
                request.CustomerId,
                request.InvoiceNumber,
                request.InvoiceDate,
                request.PaymentType
            );

            await invoiceRepository.AddAsync(salesInvoice, cancellationToken);
            await invoiceRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);


            foreach (var itemDto in request.Items)
            {
                var productVariant = await variantRepository.GetByIdAsync(itemDto.ProductVariantId, cancellationToken);
                if (productVariant == null) throw new InvalidOperationException($"محصول با شناسه {itemDto.ProductVariantId} یافت نشد.");

                salesInvoice.AddSalesItem(itemDto.ProductVariantId, itemDto.Quantity, itemDto.UnitPrice, itemDto.DiscountPercentage, itemDto.TaxPercentage);

                var inventoryTx = new InventoryTransaction(
                    itemDto.ProductVariantId,
                    productVariant,
                    request.InvoiceDate,
                    itemDto.Quantity,
                    InventoryTransactionType.Out.Id,
                    salesInvoice.Id,
                    InvoiceType.Sales
                );
                inventoryTx.SetPrices(null, itemDto.UnitPrice);

                await inventoryRepository.AddAsync(inventoryTx, cancellationToken);
            }


            if (request.PaymentType == PaymentType.Cash)
            {
                if (!request.BankAccountId.HasValue)
                    throw new InvalidOperationException("برای پرداخت نقدی یا کارتی، انتخاب حساب الزامی است.");

                var bankAccount = await bankAccountRepository.GetByIdForUpdateAsync(request.BankAccountId.Value, cancellationToken);
                if (bankAccount == null)
                    throw new InvalidOperationException("حساب بانکی یا صندوق انتخاب شده معتبر نیست.");

                var financialTx = new FinancialTransaction(
                    bankAccount,
                    salesInvoice.TotalAmount,
                    TransactionType.Debit,
                    $"پرداخت بابت فاکتور خرید شماره {salesInvoice.InvoiceNumber}",
                    salesInvoice.Id,
                    InvoiceType.Purchase
                );

                bankAccount.AddTransaction(financialTx);
                await bankAccountRepository.UpdateAsync(bankAccount, cancellationToken);

                salesInvoice.MarkAsPaid(salesInvoice.TotalAmount);

            }
            else

            if (salesInvoice.PaymentType == PaymentType.Installment && request.InstallmentDetails != null)
            {
                var details = request.InstallmentDetails;
                if (details == null)
                    throw new InvalidOperationException("برای پرداخت اقساطی، جزئیات اقساط الزامی است.");
                if (details.Months <= 0)
                    throw new InvalidOperationException("تعداد ماه‌های اقساط باید بیشتر از صفر باشد.");
                if (salesInvoice.TotalAmount < details.DownPayment)
                    throw new InvalidOperationException("مبلغ پیش‌پرداخت نمی‌تواند بیشتر از مبلغ کل فاکتور باشد.");

                // مرحله ۱: ثبت تراکنش مالی برای "پیش‌پرداخت" (در صورت وجود)
                if (details.DownPayment > 0)
                {
                    if (!request.BankAccountId.HasValue)
                        throw new InvalidOperationException("برای ثبت پیش‌پرداخت، انتخاب حساب الزامی است.");

                    var bankAccount = await bankAccountRepository.GetByIdForUpdateAsync(request.BankAccountId.Value, cancellationToken);
                    if (bankAccount == null)
                        throw new InvalidOperationException("حساب بانکی یا صندوق انتخاب شده برای پیش‌پرداخت معتبر نیست.");

                    var downPaymentTx = new FinancialTransaction(
                        bankAccount,
                        details.DownPayment,
                        TransactionType.Debit, // پرداخت از حساب ما
                        $"پیش‌پرداخت فاکتور خرید شماره {salesInvoice.InvoiceNumber}",
                        salesInvoice.Id,
                        InvoiceType.Purchase
                    );
                    bankAccount.AddTransaction(downPaymentTx);
                    await bankAccountRepository.UpdateAsync(bankAccount, cancellationToken);
                }

                salesInvoice.MarkAsPaid(details.DownPayment);

                // مرحله ۲: محاسبه و ایجاد رکوردهای اقساط
                decimal remainingAmount = salesInvoice.TotalAmount - details.DownPayment;
                if (remainingAmount > 0)
                {
                    // فرمول محاسبه مبلغ هر قسط با احتساب سود ساده ماهانه
                    decimal monthlyInterestRate = (decimal)details.InterestRate / 100;
                    decimal totalInterest = remainingAmount * monthlyInterestRate * details.Months;
                    decimal totalPayable = remainingAmount + totalInterest;
                    decimal installmentAmount = totalPayable / details.Months;

                    for (int i = 1; i <= details.Months; i++)
                    {
                        var installment = new Installment(
                            salesInvoice.Id,
                            InvoiceType.Purchase,
                            i, // شماره قسط
                            request.InvoiceDate.AddMonths(i), // تاریخ سررسید
                            installmentAmount, // مبلغ قسط
                            0 // مبلغ پرداخت شده اولیه صفر است
                        );
                        await installmentRepository.AddAsync(installment, cancellationToken);
                    }
                }
            }

            await invoiceRepository.UpdateAsync(salesInvoice, cancellationToken);
            await invoiceRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

            logger.LogInformation("فاکتور خرید با شماره {InvoiceNumber} با موفقیت ایجاد شد.", request.InvoiceNumber);

            return salesInvoice.Id;
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "خطای منطقی در هنگام ایجاد فاکتور خرید: {ErrorMessage}", ex.Message);
            throw new ApplicationException("خطایی در فرآیند ثبت فاکتور رخ داد.", ex);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "خطای پیش‌بینی نشده در هنگام ایجاد فاکتور خرید با شماره {InvoiceNumber}", request.InvoiceNumber);
            throw new ApplicationException("خطایی در فرآیند ثبت فاکتور رخ داد. لطفاً با پشتیبانی تماس بگیرید.", ex);
        }
    }
}