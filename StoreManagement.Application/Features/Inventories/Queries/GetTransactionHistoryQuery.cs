using StoreManagement.Application.DTOs.Inventories;

namespace StoreManagement.Application.Features.Inventories.Queries;

public class GetTransactionHistoryQuery : IRequest<Result<List<InventoryTransactionHistoryDto>>>
{
    public long ProductVariantId { get; set; }
    
}

public class GetTransactionHistoryQueryHandler : IRequestHandler<GetTransactionHistoryQuery, Result<List<InventoryTransactionHistoryDto>>>
{
    private readonly IDapperRepository _dapper;

    public GetTransactionHistoryQueryHandler(IDapperRepository dapper)
    {
        _dapper = dapper;
    }

    public async Task<Result<List<InventoryTransactionHistoryDto>>> Handle(GetTransactionHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // این کوئری پیچیده، تراکنش‌ها را با فاکتورهای خرید و فروش جوین می‌زند تا شماره فاکتور را استخراج کند
            const string sql = @"
SELECT 
    it.TransactionDate,
    CASE 
        WHEN itt.Id = 1 THEN N'خرید' 
        WHEN itt.Id = 2 THEN N'فروش' 
        WHEN itt.Id = 3 THEN N'اصلاحیه'
        ELSE itt.Name 
    END AS TransactionType,
    CASE WHEN itt.Id = 1 THEN it.Quantity ELSE -it.Quantity END AS QuantityChange,
    COALESCE(si.InvoiceNumber, pi.InvoiceNumber, 'N/A') AS ReferenceInvoiceNumber,
    it.Description
FROM InventoryTransactions it
INNER JOIN InventoryTransactionTypes itt ON it.TransactionTypeId = itt.Id
LEFT JOIN SaleInvoices si ON it.ReferenceInvoiceId = si.Id AND it.ReferenceInvoiceType = 1 -- Sales
LEFT JOIN PurchaseInvoices pi ON it.ReferenceInvoiceId = pi.Id AND it.ReferenceInvoiceType = 0 -- Purchase
WHERE it.ProductVariantId = @ProductVariantId AND it.IsDeleted = 0
ORDER BY it.TransactionDate DESC";

            var result = await _dapper.QueryAsync<InventoryTransactionHistoryDto>(sql, new { request.ProductVariantId }, cancellationToken: cancellationToken);
            return Result.Success(result.ToList());
        }
        catch (Exception ex)
        {
            return Result.Failure<List<InventoryTransactionHistoryDto>>($"An error occurred: {ex.Message}");
        }
    }
}