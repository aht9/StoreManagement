namespace StoreManagement.Application.Features.Invoices.Queries;

public class GetInvoiceForPrintQuery : IRequest<PrintInvoiceDto>
{
    public long InvoiceId { get; set; }
    public InvoiceType InvoiceType { get; set; }
}

public class GetInvoiceForPrintQueryHandler : IRequestHandler<GetInvoiceForPrintQuery, PrintInvoiceDto>
{
    private readonly IDapperRepository _dapper;

    public GetInvoiceForPrintQueryHandler(IDapperRepository dapper)
    {
        _dapper = dapper;
    }

    public async Task<PrintInvoiceDto> Handle(GetInvoiceForPrintQuery request, CancellationToken cancellationToken)
    {
        var invoiceTable = request.InvoiceType == InvoiceType.Sales ? "SaleInvoices" : "PurchaseInvoices";
        var itemTable = request.InvoiceType == InvoiceType.Sales ? "SalesInvoiceItems" : "PurchaseInvoiceItems";
        var partyJoin = request.InvoiceType == InvoiceType.Sales
            ? "INNER JOIN Customers p ON i.CustomerId = p.Id"
            : "INNER JOIN Stores p ON i.StoreId = p.Id";

        var partyName = request.InvoiceType == InvoiceType.Sales ? "p.FirstName+' '+ p.LastName" : "p.Name";
        var referenceName = request.InvoiceType == InvoiceType.Sales ? "ii.SalesInvoiceId" : "ii.PurchaseInvoiceId";
        
        var sql = $@"
SELECT 
    i.InvoiceNumber, i.InvoiceDate, {partyName} as BuyerName, 
    p.Address_FullAddress as BuyerAddress, p.PhoneNumber as BuyerPhone,
    i.TotalAmount as GrandTotal
FROM {invoiceTable} i {partyJoin}
WHERE i.Id = @InvoiceId;

SELECT 
    ROW_NUMBER() OVER (ORDER BY ii.Id) as RowNumber,
    prod.Name as ProductName, ii.Quantity, ii.UnitPrice,
    ii.Discount, ii.Tax
FROM {itemTable} ii
INNER JOIN ProductVariants pv ON ii.ProductVariantId = pv.Id
INNER JOIN Products prod ON pv.ProductId = prod.Id
WHERE {referenceName} = @InvoiceId;";

        using (var multi = await _dapper.QueryMultipleAsync(sql, new { request.InvoiceId }))
        {
            var invoiceDto = await multi.ReadSingleOrDefaultAsync<PrintInvoiceDto>();
            if (invoiceDto != null)
            {
                invoiceDto.Items = (await multi.ReadAsync<PrintInvoiceItemDto>()).ToList();
                    
                invoiceDto.SubTotal = invoiceDto.Items.Sum(x => x.GrossPrice);
                invoiceDto.TotalDiscount = invoiceDto.Items.Sum(x => x.DiscountAmount);
                invoiceDto.TotalTax = invoiceDto.Items.Sum(x => x.TaxAmount);
                    
                invoiceDto.GrandTotal = invoiceDto.SubTotal - invoiceDto.TotalDiscount + invoiceDto.TotalTax;
                
                //تبدیل عدد به حروف
                invoiceDto.GrandTotalInWords = NumberToWords.Convert(invoiceDto.GrandTotal) + " ریال";

            }

            return invoiceDto;
        }
    }
}