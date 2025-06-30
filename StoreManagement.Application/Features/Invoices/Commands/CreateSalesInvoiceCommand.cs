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
    IDapperRepository dapperRepository)
    : IRequestHandler<CreateSalesInvoiceCommand, long>
{
    public async Task<long> Handle(CreateSalesInvoiceCommand request, CancellationToken cancellationToken)
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
                null,
                InvoiceType.Sales
            );
            inventoryTx.SetPrices(null, itemDto.UnitPrice);

            await inventoryRepository.AddAsync(inventoryTx, cancellationToken);
        }

        await invoiceRepository.AddAsync(salesInvoice, cancellationToken);
        await invoiceRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        if (salesInvoice.PaymentType == PaymentType.Installment && request.InstallmentDetails != null)
        {
            var details = request.InstallmentDetails;
            decimal remainingAmount = salesInvoice.TotalAmount - details.DownPayment;
            if (remainingAmount <= 0 || details.Months <= 0) throw new InvalidOperationException("اطلاعات اقساط نامعتبر است.");

            decimal installmentAmount = remainingAmount / details.Months; 

            for (int i = 1; i <= details.Months; i++)
            {
                var installment = new Installment(
                    salesInvoice.Id, 
                    InvoiceType.Sales,
                    i,
                    request.InvoiceDate.AddMonths(i),
                    installmentAmount,
                    0
                );
                await installmentRepository.AddAsync(installment, cancellationToken);
            }
        }

        await invoiceRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return salesInvoice.Id;
    }
}