using StoreManagement.Application.DTOs.Inventories;

namespace StoreManagement.Application.Features.Inventories.Queries;

public class GetInventoryStatusQuery : IRequest<Result<List<InventoryStatusDto>>>
{
    public string SearchText { get; set; }
}

public class GetInventoryStatusQueryHandler : IRequestHandler<GetInventoryStatusQuery, Result<List<InventoryStatusDto>>>
{
    private readonly IDapperRepository _dapper;

    public GetInventoryStatusQueryHandler(IDapperRepository dapper)
    {
        _dapper = dapper;
    }

    public async Task<Result<List<InventoryStatusDto>>> Handle(GetInventoryStatusQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var sqlBuilder = new StringBuilder(@"
SELECT 
    i.ProductVariantId,
    p.Name AS ProductName,
    pv.SKU,
    pv.Color,
    pv.Size,
    i.Quantity AS QuantityOnHand,
    pc.Name AS CategoryName
FROM [dbo].[Inventories] i
INNER JOIN [dbo].[ProductVariants] pv ON i.ProductVariantId = pv.Id
INNER JOIN [dbo].[Products] p ON pv.ProductId = p.Id
LEFT JOIN [dbo].[ProductCategories] pc ON p.CategoryId = pc.Id
WHERE i.IsDeleted = 0 AND pv.IsDeleted = 0 AND p.IsDeleted = 0");

            var parameters = new DynamicParameters();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                sqlBuilder.Append(" AND (p.Name LIKE @SearchText OR pv.SKU LIKE @SearchText OR pc.Name LIKE @SearchText)");
                parameters.Add("SearchText", $"%{request.SearchText}%");
            }

            sqlBuilder.Append(" ORDER BY pc.Name, p.Name, pv.SKU;");

            var result = await _dapper.QueryAsync<InventoryStatusDto>(sqlBuilder.ToString(), parameters, cancellationToken: cancellationToken);
            return Result.Success(result.ToList());
        }
        catch (Exception ex)
        {
            return Result.Failure<List<InventoryStatusDto>>($"An error occurred while fetching inventory status: {ex.Message}");
        }
    }
    
}