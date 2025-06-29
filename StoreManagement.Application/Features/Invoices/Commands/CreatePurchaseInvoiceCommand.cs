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
    IGenericRepository<FinancialTransaction> financialTransactionRepository,
    IGenericRepository<BankAccount> bankAccountRepository)
    : IRequestHandler<CreatePurchaseInvoiceCommand, long>
{
    private readonly IGenericRepository<PurchaseInvoice> _invoiceRepository = invoiceRepository ?? throw new ArgumentNullException(nameof(invoiceRepository));
    private readonly IGenericRepository<InventoryTransaction> _inventoryRepository = inventoryRepository ?? throw new ArgumentNullException(nameof(inventoryRepository));
    private readonly IGenericRepository<Installment> _installmentRepository = installmentRepository ?? throw new ArgumentNullException(nameof(installmentRepository));
    private readonly IGenericRepository<FinancialTransaction> _financialTransactionRepository = financialTransactionRepository;
    private readonly IGenericRepository<ProductVariant> _variantRepository = variantRepository ?? throw new ArgumentNullException(nameof(variantRepository));


    public async Task<long> Handle(CreatePurchaseInvoiceCommand request, CancellationToken cancellationToken)
    {
        var purchaseInvoice = new PurchaseInvoice(
            request.InvoiceNumber,
            request.InvoiceDate,
            request.PaymentType,
            request.StoreId
        );

        foreach (var itemDto in request.Items)
        {
            var productVariant = await _variantRepository.GetByIdAsync(itemDto.ProductVariantId, cancellationToken);
            if (productVariant == null)
            {
                throw new InvalidOperationException($"واریانت محصول با شناسه {itemDto.ProductVariantId} یافت نشد.");
            }

            purchaseInvoice.AddPurchaseItem(
                itemDto.ProductVariantId,
                itemDto.Quantity,
                itemDto.UnitPrice,
                itemDto.DiscountPercentage,
                itemDto.TaxPercentage
            );

            decimal salePrice = itemDto.SalePriceForPurchase.HasValue && itemDto.SalePriceForPurchase.Value > 0
                ? itemDto.SalePriceForPurchase.Value
                : itemDto.UnitPrice * 1.40m;

            var inventoryTx = new InventoryTransaction(
                itemDto.ProductVariantId,
                productVariant, 
                request.InvoiceDate,
                itemDto.Quantity,
                InventoryTransactionType.In, 
                null,
                InvoiceType.Purchase
            );
             inventoryTx.SetPrices(itemDto.UnitPrice, salePrice); 

            await _inventoryRepository.AddAsync(inventoryTx, cancellationToken);
        }

        await _invoiceRepository.AddAsync(purchaseInvoice, cancellationToken);
        await _invoiceRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);


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
                    await _installmentRepository.AddAsync(installment, cancellationToken);
                }
            }
        }

        await _invoiceRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return purchaseInvoice.Id;
    }
}