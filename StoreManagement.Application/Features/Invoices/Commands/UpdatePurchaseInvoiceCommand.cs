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
            var includes = new IncludeSpecification<PurchaseInvoice>();
            includes.Include(i => i.Items);
            var invoice = await invoiceRepository.GetByIdAsync(request.InvoiceId, includes, cancellationToken);

            if (invoice == null) throw new InvalidOperationException("فاکتور خرید مورد نظر یافت نشد.");

            var oldTotalAmount = invoice.TotalAmount;

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

                if (delta == 0) continue;

                if (!inventoriesToUpdate.TryGetValue(variantId, out var inventory))
                {
                    inventory = new Inventory(variantId);
                    await inventoryRepo.AddAsync(inventory, cancellationToken);
                }

                if (delta > 0) inventory.IncreaseStock(delta);
                else inventory.DecreaseStock(-delta);

                var productVariant = await variantRepository.GetByIdAsync(variantId, cancellationToken);
                var transactionType = delta > 0 ? InventoryTransactionType.In : InventoryTransactionType.Out;

                var inventoryTx = new InventoryTransaction(
                    variantId, productVariant, DateTime.Now, Math.Abs(delta), transactionType.Id,
                    invoice.Id, InvoiceType.Purchase);
                inventoryTx.SetDescription($"اصلاحیه فاکتور خرید شماره {invoice.InvoiceNumber}");
                await inventoryRepository.AddAsync(inventoryTx, cancellationToken);
            }

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
                if (invoice.PaymentType == PaymentType.Cash || (invoice.PaymentType == PaymentType.Installment && !await installmentRepository.AnyAsync(new CustomExpressionSpecification<Installment>(i => i.InvoiceId == invoice.Id && i.Status != InstallmentStatus.Paid), cancellationToken)))
                {
                    if (!request.BankAccountIdForAdjustment.HasValue)
                        throw new InvalidOperationException("برای ثبت اصلاحیه، انتخاب حساب بانکی الزامی است.");

                    var bankAccount = await bankAccountRepository.GetByIdForUpdateAsync(request.BankAccountIdForAdjustment.Value, cancellationToken);
                    if (bankAccount == null) throw new InvalidOperationException("حساب بانکی مورد نظر برای ثبت اصلاحیه یافت نشد.");

                    var adjustmentDirection = amountDifference > 0 ? TransactionType.Debit : TransactionType.Credit;
                    var adjustmentAmount = Math.Abs(amountDifference);
                    var description = $"اصلاحیه فاکتور خرید شماره {invoice.InvoiceNumber}";

                    var adjustmentTx = new FinancialTransaction(bankAccount, adjustmentAmount, adjustmentDirection, description, invoice.Id, InvoiceType.Purchase);
                    bankAccount.AddTransaction(adjustmentTx);
                    await bankAccountRepository.UpdateAsync(bankAccount, cancellationToken);
                }
                else if (invoice.PaymentType == PaymentType.Installment)
                {
                    var installmentsSpec = new CustomExpressionSpecification<Installment>(i => i.InvoiceId == invoice.Id && i.InvoiceType == InvoiceType.Purchase && i.Status != InstallmentStatus.Paid);
                    var unpaidInstallments = await installmentRepository.ListAsync(installmentsSpec, (OrderBySpecification<Installment, object>)null, cancellationToken);

                    if (unpaidInstallments.Any())
                    {
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