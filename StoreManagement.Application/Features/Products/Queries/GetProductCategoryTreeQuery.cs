namespace StoreManagement.Application.Features.Products.Queries;

public class GetProductCategoryTreeQuery : IRequest<Result<List<ProductCategoryTreeDto>>>
{
}


public class GetProductCategoryTreeQueryHandler(IDapperRepository dapper, ILogger<GetProductCategoryTreeQueryHandler> logger)
    : IRequestHandler<GetProductCategoryTreeQuery, Result<List<ProductCategoryTreeDto>>>
{

    public async Task<Result<List<ProductCategoryTreeDto>>> Handle(GetProductCategoryTreeQuery request,
        CancellationToken cancellationToken)
    {
        const string sql = @"
WITH CategoryHierarchy AS (
    -- Anchor member: Select all root categories (those with no parent)
    SELECT 
        Id,
        Name,
        Description,
        [Order],
        ParentCategoryId
    FROM 
        ProductCategories
    WHERE 
        ParentCategoryId IS NULL AND IsDeleted = 0

    UNION ALL

    -- Recursive member: Join subcategories to their parents
    SELECT 
        c.Id,
        c.Name,
        c.Description,
        c.[Order],
        c.ParentCategoryId
    FROM 
        ProductCategories c
    INNER JOIN 
        CategoryHierarchy ch ON c.ParentCategoryId = ch.Id
    WHERE 
        c.IsDeleted = 0
)
SELECT 
    * FROM 
    CategoryHierarchy
ORDER BY 
    [Order], Name;";

        try
        {
            var allCategories = await dapper.QueryAsync<ProductCategoryTreeDto>(sql, cancellationToken: cancellationToken);
            var categoryMap = allCategories.ToDictionary(c => c.Id);
            var rootCategories = new List<ProductCategoryTreeDto>();
            foreach (var category in allCategories)
            {
                if (category.ParentCategoryId.HasValue && categoryMap.TryGetValue(category.ParentCategoryId.Value, out var parent))
                {
                    parent.Subcategories.Add(category);
                }
                else
                {
                    rootCategories.Add(category);
                }
            }

            return Result.Success(rootCategories);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error querying all Product Category.");
            return Result.Failure<List<ProductCategoryTreeDto>>("خطای سیستمی هنگام دریافت لیست محصولات رخ داد.");
        }
    }
}