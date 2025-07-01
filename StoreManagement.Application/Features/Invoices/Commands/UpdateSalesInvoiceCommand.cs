namespace StoreManagement.Application.Features.Invoices.Commands;

public class UpdateSalesInvoiceCommand : IRequest
{
    public long InvoiceId { get; set; }
    public string InvoiceNumber { get; set; }
    public DateTime InvoiceDate { get; set; }
    public long CustomerId { get; set; }
    public List<InvoiceItemDto> Items { get; set; }
    public long? BankAccountId { get; set; }
}


public class UpdateSalesInvoiceCommandHandler(
    IGenericRepository<SalesInvoice> invoiceRepository,
    IGenericRepository<Inventory> inventoryRepo,
    IGenericRepository<InventoryTransaction> inventoryTransactionRepository,
    IGenericRepository<ProductVariant> variantRepository,
    IGenericRepository<FinancialTransaction> financialTransactionRepository,
    IGenericRepository<BankAccount> bankAccountRepository,
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
            var inventoriesToUpdate = (await inventoryRepo.ListAsync(inventorySpec, null as IncludeSpecification<Inventory>, cancellationToken)).ToDictionary(i => i.ProductVariantId);

            foreach (var variantId in allVariantIds)
            {
                var oldQty = oldItems.TryGetValue(variantId, out var o) ? o : 0;
                var newQty = newItems.TryGetValue(variantId, out var n) ? n : 0;
                var delta = newQty - oldQty;

                if (delta == 0) continue;

                if (!inventoriesToUpdate.TryGetValue(variantId, out var inventory))
                    throw new InvalidOperationException($"موجودی انبار برای کالای {variantId} یافت نشد.");

                if (delta > 0) inventory.DecreaseStock(delta);
                else inventory.IncreaseStock(-delta);

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
            if (amountDifference != 0 && (invoice.PaymentType == PaymentType.Cash))
            {
                // این منطق فقط برای پرداخت‌های نقدی/کارتی اعمال می‌شود
                var financialSpec = new CustomExpressionSpecification<FinancialTransaction>(
                    ft => ft.InvoiceId == invoice.Id && ft.InvoiceType == InvoiceType.Sales);
                var originalTx = await financialTransactionRepository.FirstOrDefaultAsync(financialSpec, cancellationToken);

                if (originalTx == null)
                {
                    // برای ثبت تراکنش اولیه، باید حساب بانکی مشخص شده باشد
                    if (!request.BankAccountId.HasValue)
                        throw new InvalidOperationException("برای ثبت پرداخت، انتخاب حساب الزامی است.");

                    var bankAccount = await bankAccountRepository.GetByIdForUpdateAsync(request.BankAccountId.Value, cancellationToken);
                    if (bankAccount == null) throw new InvalidOperationException("حساب بانکی مورد نظر یافت نشد.");

                    // یک تراکنش جدید برای کل مبلغ جدید فاکتور ایجاد می‌کنیم
                    var newFinancialTx = new FinancialTransaction(
                        bankAccount,
                        invoice.TotalAmount, // مبلغ کل جدید
                        TransactionType.Credit, // برای فاکتور فروش، پول به حساب ما واریز می‌شود
                        $"ثبت پرداخت بابت فاکتور فروش شماره {invoice.InvoiceNumber}",
                        invoice.Id,
                        InvoiceType.Sales
                    );
                    bankAccount.AddTransaction(newFinancialTx);
                    await bankAccountRepository.UpdateAsync(bankAccount, cancellationToken);
                }
                else
                {
                    var bankAccount = await bankAccountRepository.GetByIdForUpdateAsync(originalTx.BankAccountId, cancellationToken);
                    if (bankAccount == null) throw new InvalidOperationException("حساب بانکی مرتبط یافت نشد.");

                    //  ایجاد یک تراکنش معکوس برای خنثی کردن تراکنش اولیه
                    var reversalDirection = originalTx.TransactionType == TransactionType.Credit ? TransactionType.Debit : TransactionType.Credit;
                    var reversalTx = new FinancialTransaction(
                        bankAccount,
                        originalTx.Amount, // معکوس کردن با همان مبلغ اولیه
                        reversalDirection,
                        $"معکوس کردن تراکنش اولیه بابت اصلاح فاکتور شماره {invoice.InvoiceNumber}",
                        invoice.Id,
                        InvoiceType.Sales
                    );
                    bankAccount.AddTransaction(reversalTx);

                    var correctTx = new FinancialTransaction(
                        bankAccount,
                        invoice.TotalAmount, // ثبت تراکنش جدید با مبلغ کل جدید
                        originalTx.TransactionType, // جهت تراکنش جدید، مانند جهت تراکنش اولیه است
                        $"ثبت مجدد تراکنش بابت اصلاح فاکتور شماره {invoice.InvoiceNumber}",
                        invoice.Id,
                        InvoiceType.Sales
                    );
                    bankAccount.AddTransaction(correctTx);

                    await bankAccountRepository.UpdateAsync(bankAccount, cancellationToken);
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