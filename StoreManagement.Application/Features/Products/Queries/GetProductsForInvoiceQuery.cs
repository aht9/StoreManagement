namespace StoreManagement.Application.Features.Products.Queries;
public class GetProductsForInvoiceQuery : IRequest<IEnumerable<ProductSearchResultDto>>
{
    public string SearchTerm { get; set; }
}

public class GetProductsForInvoiceQueryHandler(IDapperRepository dapper)
    : IRequestHandler<GetProductsForInvoiceQuery, IEnumerable<ProductSearchResultDto>>
{
    private readonly IDapperRepository _dapper = dapper ?? throw new ArgumentNullException(nameof(dapper));

    public async Task<IEnumerable<ProductSearchResultDto>> Handle(GetProductsForInvoiceQuery request, CancellationToken cancellationToken)
    {
        var searchTermParam = string.IsNullOrWhiteSpace(request.SearchTerm) ? null : $"%{request.SearchTerm}%";
        var parameters = new { SearchTerm = searchTermParam };

        const string sql = @"
WITH LatestSalePriceCTE AS (
    SELECT
        it.ProductVariantId, it.SalePrice,
        ROW_NUMBER() OVER(PARTITION BY it.ProductVariantId ORDER BY it.TransactionDate DESC) as rn
    FROM InventoryTransactions it
    WHERE it.SalePrice IS NOT NULL AND it.TransactionTypeId = 2 -- 2 = Out (Sales)
),
StockCTE AS (
    SELECT
        it.ProductVariantId,
        SUM(CASE 
                WHEN it.TransactionTypeId = 1 THEN it.Quantity -- 1 = In
                WHEN it.TransactionTypeId = 2 THEN -it.Quantity -- 2 = Out
                ELSE 0 
            END) as CurrentStock
    FROM InventoryTransactions it
    GROUP BY it.ProductVariantId
)
SELECT
    p.Id AS ProductId, pv.Id AS VariantId, p.Name, pv.SKU,
    pv.Color, pv.Size, COALESCE(s.CurrentStock, 0) AS Stock, lsp.SalePrice AS LastSalePrice
FROM Products p
INNER JOIN ProductVariants pv ON p.Id = pv.ProductId
LEFT JOIN StockCTE s ON pv.Id = s.ProductVariantId
LEFT JOIN LatestSalePriceCTE lsp ON pv.Id = lsp.ProductVariantId AND lsp.rn = 1
WHERE (@SearchTerm IS NULL OR p.Name LIKE @SearchTerm OR pv.SKU LIKE @SearchTerm)
ORDER BY p.Name, pv.Color, pv.Size;";

        return await _dapper.QueryAsync<ProductSearchResultDto>(sql, parameters, cancellationToken: cancellationToken);
    }
}