namespace StoreManagement.Application.Features.Invoices.Commands;

public class UpdatePurchaseInvoiceCommand : IRequest
{
    public long InvoiceId { get; set; }
    public string InvoiceNumber { get; set; }
    public DateTime InvoiceDate { get; set; }
    public long StoreId { get; set; }
    public List<InvoiceItemDto> Items { get; set; }
    public long? BankAccountIdForAdjustment { get; set; }

}

public class UpdatePurchaseInvoiceCommandHandler(
    IGenericRepository<PurchaseInvoice> invoiceRepository,
    IGenericRepository<InventoryTransaction> inventoryRepository,
    IGenericRepository<Inventory> inventoryRepo,
    IGenericRepository<ProductVariant> variantRepository,
    IGenericRepository<FinancialTransaction> financialTransactionRepository,
    IGenericRepository<Installment> installmentRepository,
    IGenericRepository<BankAccount> bankAccountRepository,
    ILogger<UpdatePurchaseInvoiceCommandHandler> logger)
    : IRequestHandler<UpdatePurchaseInvoiceCommand>
{
    public async Task Handle(UpdatePurchaseInvoiceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // مرحله ۱: واکشی فاکتور موجود به همراه آیتم‌های آن
            var includes = new IncludeSpecification<PurchaseInvoice>();
            includes.Include(i => i.Items);
            var invoice = await invoiceRepository.GetByIdAsync(request.InvoiceId, includes, cancellationToken);

            if (invoice == null)
                throw new InvalidOperationException("فاکتور خرید مورد نظر یافت نشد.");

            // ذخیره مبلغ کل قدیمی برای محاسبه تفاوت
            var oldTotalAmount = invoice.TotalAmount;

            // مرحله ۲: ذخیره وضعیت قدیمی و محاسبه تفاوت‌های انبار
            var oldItems = invoice.Items.ToDictionary(i => i.ProductVariantId, i => i.Quantity);
            var newItems = request.Items.ToDictionary(i => i.ProductVariantId, i => i.Quantity);
            var allVariantIds = oldItems.Keys.Union(newItems.Keys).ToList();

            var inventorySpec = new CustomExpressionSpecification<Inventory>(i => allVariantIds.Contains(i.ProductVariantId));
            var inventoriesToUpdate = (await inventoryRepo.ListAsync(inventorySpec, (IncludeSpecification<Inventory>)null, cancellationToken)).ToDictionary(i => i.ProductVariantId);

            foreach (var variantId in allVariantIds)
            {
                var oldQty = oldItems.TryGetValue(variantId, out var o) ? o : 0;
                var newQty = newItems.TryGetValue(variantId, out var n) ? n : 0;
                var delta = newQty - oldQty;

                if (delta == 0) continue; // اگر تغییری در تعداد نبود، کاری انجام نده

                // ۲.۱: به‌روزرسانی موجودی کل
                if (!inventoriesToUpdate.TryGetValue(variantId, out var inventory))
                {
                    // این حالت نباید رخ دهد اگر فاکتور به درستی ثبت شده باشد
                    inventory = new Inventory(variantId);
                    await inventoryRepo.AddAsync(inventory, cancellationToken);
                }

                if (delta > 0) inventory.IncreaseStock(delta);
                else inventory.DecreaseStock(-delta);

                // ۲.۲: ایجاد تراکنش انبار اصلاحی برای ثبت سابقه تغییر
                var productVariant = await variantRepository.GetByIdAsync(variantId, cancellationToken);
                var transactionType = delta > 0 ? InventoryTransactionType.In : InventoryTransactionType.Out;

                var inventoryTx = new InventoryTransaction(
                    variantId, productVariant, DateTime.Now, Math.Abs(delta), transactionType.Id,
                    invoice.Id, InvoiceType.Purchase);
                inventoryTx.SetPrices(null, null); // قیمت‌ها در تراکنش اصلاحی می‌تواند null باشد

                await inventoryRepository.AddAsync(inventoryTx, cancellationToken);
            }


            // مرحله ۳: به‌روزرسانی اطلاعات اصلی فاکتور و آیتم‌های آن
            invoice.UpdateDetailsPurchase(request.InvoiceNumber, request.InvoiceDate, request.StoreId);
            invoice.ClearItems();
            foreach (var itemDto in request.Items)
            {
                invoice.AddPurchaseItem(
                    itemDto.ProductVariantId,
                    itemDto.Quantity,
                    itemDto.UnitPrice,
                    itemDto.DiscountPercentage,
                    itemDto.TaxPercentage
                );
            }

            var amountDifference = invoice.TotalAmount - oldTotalAmount;

            if (amountDifference != 0)
            {
                // اگر پرداخت از نوع نقدی/کارتی بود
                if (invoice.PaymentType == PaymentType.Cash)
                {
                    // باید حساب بانکی مرتبط با تراکنش اولیه را پیدا کنیم
                    var originalFinancialTxSpec = new CustomExpressionSpecification<FinancialTransaction>(
                        ft => ft.InvoiceId == invoice.Id && ft.InvoiceType == InvoiceType.Purchase);
                    var originalTx = await financialTransactionRepository.FirstOrDefaultAsync(originalFinancialTxSpec, cancellationToken);

                    if (originalTx == null) throw new InvalidOperationException("تراکنش مالی اولیه برای این فاکتور یافت نشد.");

                    var bankAccount = await bankAccountRepository.GetByIdForUpdateAsync(originalTx.BankAccountId, cancellationToken);
                    if (bankAccount == null) throw new InvalidOperationException("حساب بانکی مرتبط با تراکنش اولیه یافت نشد.");

                    var adjustmentDirection = amountDifference > 0 ? TransactionType.Debit : TransactionType.Credit;
                    var adjustmentAmount = Math.Abs(amountDifference);
                    var description = $"اصلاحیه فاکتور خرید شماره {invoice.InvoiceNumber}";

                    var adjustmentTx = new FinancialTransaction(bankAccount, adjustmentAmount, adjustmentDirection, description, invoice.Id, InvoiceType.Purchase);
                    bankAccount.AddTransaction(adjustmentTx);
                    await bankAccountRepository.UpdateAsync(bankAccount, cancellationToken);
                }
                // اگر پرداخت از نوع اقساطی بود
                else if (invoice.PaymentType == PaymentType.Installment)
                {
                    var installmentsSpec = new CustomExpressionSpecification<Installment>(i => i.InvoiceId == invoice.Id && i.InvoiceType == InvoiceType.Purchase);
                    var installments = await installmentRepository.ListAsync(installmentsSpec, (IncludeSpecification<Installment>)null, cancellationToken);

                    var unpaidInstallments = installments.Where(i => i.Status != InstallmentStatus.Paid).OrderBy(i => i.InstallmentNumber).ToList();
                    if (!unpaidInstallments.Any())
                    {
                        // اگر تمام اقساط پرداخت شده باشند، باید یک تراکنش مالی جداگانه برای تفاوت ثبت شود
                        logger.LogWarning("تغییر در مبلغ فاکتور اقساطی که تمام اقساط آن پرداخت شده است. InvoiceId: {InvoiceId}", invoice.Id);
                        if (!request.BankAccountIdForAdjustment.HasValue)
                            throw new InvalidOperationException("برای ثبت اصلاحیه فاکتور پرداخت شده، انتخاب حساب الزامی است.");

                        var bankAccount = await bankAccountRepository.GetByIdForUpdateAsync(request.BankAccountIdForAdjustment.Value, cancellationToken);
                        if (bankAccount == null) throw new InvalidOperationException("حساب بانکی مورد نظر برای ثبت اصلاحیه یافت نشد.");

                        // تعیین جهت تراکنش: اگر مبلغ زیاد شده، ما بدهکاریم (Debit)، اگر کم شده، بستانکاریم (Credit)
                        var adjustmentDirection = amountDifference > 0 ? TransactionType.Debit : TransactionType.Credit;
                        var adjustmentAmount = Math.Abs(amountDifference);
                        var description = $"اصلاحیه مبلغ فاکتور شماره {invoice.InvoiceNumber}";

                        var adjustmentTx = new FinancialTransaction(bankAccount, adjustmentAmount, adjustmentDirection, description, invoice.Id, InvoiceType.Purchase);
                        bankAccount.AddTransaction(adjustmentTx);
                        await bankAccountRepository.UpdateAsync(bankAccount, cancellationToken);
                    }
                    else
                    {
                        // تفاوت مبلغ را بین اقساط پرداخت نشده تقسیم کن
                        var adjustmentPerInstallment = amountDifference / unpaidInstallments.Count;
                        foreach (var installment in unpaidInstallments)
                        {
                            installment.UpdateAmountDue(installment.AmountDue + adjustmentPerInstallment);
                            await installmentRepository.UpdateAsync(installment, cancellationToken);
                        }
                    }
                }
            }

            await invoiceRepository.UpdateAsync(invoice, cancellationToken);
            await invoiceRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "خطای پیش‌بینی نشده در هنگام ویرایش فاکتور خرید {InvoiceId}", request.InvoiceId);
            throw new ApplicationException("خطایی در فرآیند ویرایش فاکتور رخ داد.", ex);
        }
    }
}