namespace StoreManagement.Application.Features.Invoices.Queries;

public class GetInvoiceForEditQuery : IRequest<InvoiceForEditDto>
{
    public long InvoiceId { get; set; }
    public InvoiceType InvoiceType { get; set; }
}

public class GetInvoiceForEditQueryHandler(IDapperRepository dapper)
    : IRequestHandler<GetInvoiceForEditQuery, InvoiceForEditDto>
{
    public async Task<InvoiceForEditDto> Handle(GetInvoiceForEditQuery request, CancellationToken cancellationToken)
    {
        var invoiceTable = request.InvoiceType == InvoiceType.Sales ? "SalesInvoices" : "PurchaseInvoices";
        var itemTable = request.InvoiceType == InvoiceType.Sales ? "SalesInvoiceItems" : "PurchaseInvoiceItems";
        var partyJoin = request.InvoiceType == InvoiceType.Sales
            ? "INNER JOIN Customers p ON i.CustomerId = p.Id"
            : "INNER JOIN Stores p ON i.StoreId = p.Id";
        var partyIdField = request.InvoiceType == InvoiceType.Sales ? "i.CustomerId" : "i.StoreId";

        var sql = $@"
SELECT 
    i.Id, i.InvoiceNumber, i.InvoiceDate, {partyIdField} as PartyId, p.Name as PartyName, i.PaymentType
FROM {invoiceTable} i {partyJoin}
WHERE i.Id = @InvoiceId AND i.IsDeleted = 0;

SELECT 
    ii.ProductVariantId, 
    prod.Name as ProductName, -- واکشی نام محصول
    ii.Quantity, 
    ii.UnitPrice, 
    ii.Discount as DiscountPercentage, 
    ii.Tax as TaxPercentage
FROM {itemTable} ii
INNER JOIN ProductVariants pv ON ii.ProductVariantId = pv.Id
INNER JOIN Products prod ON pv.ProductId = prod.Id
WHERE ii.InvoiceId = @InvoiceId;";

        await using var multi = await dapper.QueryMultipleAsync(sql, new { request.InvoiceId }, cancellationToken: cancellationToken);
        var invoiceDto = await multi.ReadSingleOrDefaultAsync<InvoiceForEditDto>();
        if (invoiceDto != null)
        {
            invoiceDto.Items = (await multi.ReadAsync<InvoiceItemDto>()).ToList();
        }
        return invoiceDto;
    }
}