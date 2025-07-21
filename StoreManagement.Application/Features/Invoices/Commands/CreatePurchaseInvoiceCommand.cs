namespace StoreManagement.Application.Features.Invoices.Commands;

public class CreatePurchaseInvoiceCommand : IRequest<long>
{
    public long StoreId { get; set; }

    public string InvoiceNumber { get; set; }

    public DateTime InvoiceDate { get; set; }

    public PaymentType PaymentType { get; set; }

    public long? BankAccountId { get; set; }

    public List<InvoiceItemDto> Items { get; set; }

    public InstallmentDetailsDto? InstallmentDetails { get; set; }
}


public class CreatePurchaseInvoiceCommandHandler(
    IGenericRepository<PurchaseInvoice> invoiceRepository,
    IGenericRepository<InventoryTransaction> inventoryRepository,
    IGenericRepository<Installment> installmentRepository,
    IGenericRepository<ProductVariant> variantRepository,
    IGenericRepository<BankAccount> bankAccountRepository,
    IGenericRepository<Inventory> inventoryRepo,
    ILogger<CreatePurchaseInvoiceCommandHandler> logger)
    : IRequestHandler<CreatePurchaseInvoiceCommand, long>
{
    public async Task<long> Handle(CreatePurchaseInvoiceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var purchaseInvoice = new PurchaseInvoice(
            request.InvoiceNumber,
            request.InvoiceDate,
            request.PaymentType,
            request.StoreId);

            await invoiceRepository.AddAsync(purchaseInvoice, cancellationToken);
            await invoiceRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

            var quantityChanges = request.Items
                .GroupBy(item => item.ProductVariantId)
                .ToDictionary(group => group.Key, group => group.Sum(item => item.Quantity));

            var variantIds = quantityChanges.Keys.ToList();
            var inventorySpec = new CustomExpressionSpecification<Inventory>(i => variantIds.Contains(i.ProductVariantId));
            var existingInventories = await inventoryRepo.ListAsync(
                inventorySpec,
                (OrderBySpecification<Inventory, object>?)null,
                cancellationToken
            );
            var inventoryDict = existingInventories.ToDictionary(i => i.ProductVariantId);

            foreach (var change in quantityChanges)
            {
                if (inventoryDict.TryGetValue(change.Key, out var inventory))
                {
                    if (inventory.IsDeleted)
                    {
                        inventory.Restore();
                        await inventoryRepo.UpdateAsync(inventory, cancellationToken); 
                    }
                    inventory.IncreaseStock(change.Value);
                    await inventoryRepo.UpdateAsync(inventory, cancellationToken);
                }
                else
                {
                    var newInventory = new Inventory(change.Key, change.Value);
                    await inventoryRepo.AddAsync(newInventory, cancellationToken);
                }
            }

            foreach (var itemDto in request.Items)
            {
                var productVariant = await variantRepository.GetByIdAsync(itemDto.ProductVariantId, cancellationToken);
                if (productVariant == null) throw new InvalidOperationException($"واریانت محصول با شناسه {itemDto.ProductVariantId} یافت نشد.");

                var purchaseItem = new PurchaseInvoiceItem(
                    0,
                    itemDto.ProductVariantId,
                    itemDto.Quantity,
                    itemDto.UnitPrice,
                    itemDto.DiscountPercentage,
                    itemDto.TaxPercentage
                );
                purchaseInvoice.AddItem(purchaseItem);


                var inventoryTx = new InventoryTransaction(
                    itemDto.ProductVariantId, productVariant, request.InvoiceDate, itemDto.Quantity,
                    InventoryTransactionType.In.Id, purchaseInvoice.Id, InvoiceType.Purchase); 
                inventoryTx.SetPrices(itemDto.UnitPrice, itemDto.SalePriceForPurchase ?? (itemDto.UnitPrice * 1.40m));

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
                    purchaseInvoice.TotalAmount,
                    TransactionType.Debit,
                    $"پرداخت بابت فاکتور خرید شماره {purchaseInvoice.InvoiceNumber}",
                    purchaseInvoice.Id,
                    InvoiceType.Purchase
                );

                bankAccount.AddTransaction(financialTx);
                await bankAccountRepository.UpdateAsync(bankAccount, cancellationToken);

                purchaseInvoice.MarkAsPaid(purchaseInvoice.TotalAmount);

            }

            //  مدیریت پرداخت اقساطی (در صورت وجود)
            else if (purchaseInvoice.PaymentType == PaymentType.Installment && request.InstallmentDetails != null)
            {
                var details = request.InstallmentDetails;
                if (details == null)
                    throw new InvalidOperationException("برای پرداخت اقساطی، جزئیات اقساط الزامی است.");
                if (details.Months <= 0)
                    throw new InvalidOperationException("تعداد ماه‌های اقساط باید بیشتر از صفر باشد.");
                if (purchaseInvoice.TotalAmount < details.DownPayment)
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
                        $"پیش‌پرداخت فاکتور خرید شماره {purchaseInvoice.InvoiceNumber}",
                        purchaseInvoice.Id,
                        InvoiceType.Purchase
                    );
                    bankAccount.AddTransaction(downPaymentTx);
                    await bankAccountRepository.UpdateAsync(bankAccount, cancellationToken);
                }

                purchaseInvoice.MarkAsPaid(details.DownPayment);

                // مرحله ۲: محاسبه و ایجاد رکوردهای اقساط
                decimal remainingAmount = purchaseInvoice.TotalAmount - details.DownPayment;
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
                            purchaseInvoice.Id,
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

            await invoiceRepository.UpdateAsync(purchaseInvoice, cancellationToken);
            await invoiceRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

            logger.LogInformation("فاکتور خرید با شماره {InvoiceNumber} با موفقیت ایجاد شد.", request.InvoiceNumber);

            return purchaseInvoice.Id;
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

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException is SqlException sqlEx &&
               (sqlEx.Number == 2627 || sqlEx.Number == 2601); // SQL Server unique constraint violation numbers
    }
}