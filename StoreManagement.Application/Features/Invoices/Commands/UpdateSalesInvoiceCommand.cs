namespace StoreManagement.Application.Features.Invoices.Commands;

public class UpdateSalesInvoiceCommand : IRequest
{
    public long InvoiceId { get; set; }
    public string InvoiceNumber { get; set; }
    public DateTime InvoiceDate { get; set; }
    public long CustomerId { get; set; }
    public List<InvoiceItemDto> Items { get; set; }
    public long? BankAccountIdForAdjustment { get; set; }
}


public class UpdateSalesInvoiceCommandHandler(
        IGenericRepository<SalesInvoice> invoiceRepository,
        IGenericRepository<Inventory> inventoryRepo,
        IGenericRepository<InventoryTransaction> inventoryTransactionRepository,
        IGenericRepository<ProductVariant> variantRepository,
        IGenericRepository<FinancialTransaction> financialTransactionRepository,
        IGenericRepository<BankAccount> bankAccountRepository,
        IGenericRepository<Installment> installmentRepository, // اضافه کردن ریپازیتوری اقساط
        ILogger<UpdateSalesInvoiceCommandHandler> logger)
        : IRequestHandler<UpdateSalesInvoiceCommand>
{
    public async Task Handle(UpdateSalesInvoiceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var includes = new IncludeSpecification<SalesInvoice>();
            includes.Include(i => i.Items);
            var invoice = await invoiceRepository.GetByIdAsync(request.InvoiceId, includes, cancellationToken);
            if (invoice == null) throw new InvalidOperationException("فاکتور فروش مورد نظر یافت نشد.");

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
                    throw new InvalidOperationException($"موجودی انبار برای کالای {variantId} یافت نشد.");

                // برای فاکتور فروش، منطق برعکس فاکتور خرید است
                if (delta > 0) inventory.DecreaseStock(delta); // افزایش تعداد در فاکتور -> کاهش موجودی
                else inventory.IncreaseStock(-delta); // کاهش تعداد در فاکتور -> افزایش موجودی

                var productVariant = await variantRepository.GetByIdAsync(variantId, cancellationToken);
                var transactionType = delta > 0 ? InventoryTransactionType.Out : InventoryTransactionType.In;
                var description = $"اصلاحیه فاکتور فروش شماره {invoice.InvoiceNumber}";

                var adjustmentTx = new InventoryTransaction(
                    variantId, productVariant, DateTime.Now, Math.Abs(delta), transactionType.Id,
                    invoice.Id, InvoiceType.Sales);
                adjustmentTx.SetDescription(description);

                await inventoryTransactionRepository.AddAsync(adjustmentTx, cancellationToken);
            }

            invoice.UpdateDetailSale(request.InvoiceNumber, request.InvoiceDate, request.CustomerId);
            invoice.ClearItems();
            foreach (var itemDto in request.Items)
            {
                invoice.AddSalesItem(itemDto.ProductVariantId, itemDto.Quantity, itemDto.UnitPrice, itemDto.DiscountPercentage, itemDto.TaxPercentage);
            }

            await invoiceRepository.UpdateAsync(invoice, cancellationToken);

            var amountDifference = invoice.TotalAmount - oldTotalAmount;
            if (amountDifference != 0)
            {
                if (invoice.PaymentType == PaymentType.Cash || (invoice.PaymentType == PaymentType.Installment && !await installmentRepository.AnyAsync(new CustomExpressionSpecification<Installment>(i => i.InvoiceId == invoice.Id && i.Status != InstallmentStatus.Paid), cancellationToken)))
                {
                    if (!request.BankAccountIdForAdjustment.HasValue)
                        throw new InvalidOperationException("برای ثبت اصلاحیه، انتخاب حساب الزامی است.");

                    var bankAccount = await bankAccountRepository.GetByIdForUpdateAsync(request.BankAccountIdForAdjustment.Value, cancellationToken);
                    if (bankAccount == null) throw new InvalidOperationException("حساب بانکی مورد نظر یافت نشد.");

                    var adjustmentDirection = amountDifference > 0 ? TransactionType.Credit : TransactionType.Debit;
                    var adjustmentAmount = Math.Abs(amountDifference);
                    var description = $"اصلاحیه فاکتور فروش شماره {invoice.InvoiceNumber}";

                    var adjustmentTx = new FinancialTransaction(bankAccount, adjustmentAmount, adjustmentDirection, description, invoice.Id, InvoiceType.Sales);
                    bankAccount.AddTransaction(adjustmentTx);
                    await bankAccountRepository.UpdateAsync(bankAccount, cancellationToken);
                }
                else if (invoice.PaymentType == PaymentType.Installment)
                {
                    var installmentsSpec = new CustomExpressionSpecification<Installment>(i => i.InvoiceId == invoice.Id && i.InvoiceType == InvoiceType.Sales && i.Status != InstallmentStatus.Paid);
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

            await invoiceRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            logger.LogInformation("فاکتور فروش با شناسه {InvoiceId} با موفقیت ویرایش شد.", invoice.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "خطا در هنگام ویرایش فاکتور فروش با شناسه {InvoiceId}", request.InvoiceId);
            throw;
        }
    }
}