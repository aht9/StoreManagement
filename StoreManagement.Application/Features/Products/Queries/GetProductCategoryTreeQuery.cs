using System.Formats.Tar;

namespace StoreManagement.Application.Features.Products.Queries;

public class GetProductCategoryTreeQuery : IRequest<Result<List<ProductCategoryTreeDto>>>
{
    public string? SearchText { get; set; }
}


public class GetProductCategoryTreeQueryHandler(IDapperRepository dapper, ILogger<GetProductCategoryTreeQueryHandler> logger)
    : IRequestHandler<GetProductCategoryTreeQuery, Result<List<ProductCategoryTreeDto>>>
{

    public async Task<Result<List<ProductCategoryTreeDto>>> Handle(GetProductCategoryTreeQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var sqlBuilder = new StringBuilder();
            var parameters = new DynamicParameters();
            if (string.IsNullOrWhiteSpace(request.SearchText))
            {
                sqlBuilder.Append(@"
                        WITH CategoryHierarchy AS (
                            SELECT Id, Name, Description, [Order], ParentCategoryId
                            FROM ProductCategories
                            WHERE ParentCategoryId IS NULL AND IsDeleted = 0
                            UNION ALL
                            SELECT c.Id, c.Name, c.Description, c.[Order], c.ParentCategoryId
                            FROM ProductCategories c
                            INNER JOIN CategoryHierarchy ch ON c.ParentCategoryId = ch.Id
                            WHERE c.IsDeleted = 0
                        )
                        SELECT * FROM CategoryHierarchy ORDER BY [Order], Name;");
            }
            else
            {
                sqlBuilder.Append(@"
                        -- Step 1: Find all categories matching the search text and all their descendants.
                        WITH MatchingAndDescendants AS (
                            -- Anchor: Categories that directly match the search text.
                            SELECT Id, Name, Description, [Order], ParentCategoryId
                            FROM ProductCategories
                            WHERE (Name LIKE @SearchText OR Description LIKE @SearchText) AND IsDeleted = 0
                            UNION ALL
                            -- Recursive: Find all children of the categories found above.
                            SELECT c.Id, c.Name, c.Description, c.[Order], c.ParentCategoryId
                            FROM ProductCategories c
                            INNER JOIN MatchingAndDescendants d ON c.ParentCategoryId = d.Id
                            WHERE c.IsDeleted = 0
                        ),
                        -- Step 2: Find all ancestors of the categories found in Step 1.
                        MatchingAndAncestors AS (
                            -- Anchor: The set of matching categories and their descendants from the previous CTE.
                            SELECT Id, Name, Description, [Order], ParentCategoryId
                            FROM MatchingAndDescendants
                            UNION ALL
                            -- Recursive: Find all parents of the categories found above, climbing up the tree.
                            SELECT p.Id, p.Name, p.Description, p.[Order], p.ParentCategoryId
                            FROM ProductCategories p
                            INNER JOIN MatchingAndAncestors a ON p.Id = a.ParentCategoryId
                            WHERE p.IsDeleted = 0
                        )
                        -- Final Step: Select all unique categories found and order them.
                        SELECT DISTINCT Id, Name, Description, [Order], ParentCategoryId
                        FROM MatchingAndAncestors
                        ORDER BY [Order], Name;");

                parameters.Add("SearchText", $"%{request.SearchText}%");
            }

            var allCategories = await dapper.QueryAsync<ProductCategoryTreeDto>(
                sqlBuilder.ToString(), parameters, cancellationToken: cancellationToken);

            var categoryMap = allCategories.ToDictionary(c => c.Id);

            var rootCategories = new List<ProductCategoryTreeDto>();

            foreach (var category in allCategories)
            {
                if (category.ParentCategoryId.HasValue &&
                    categoryMap.TryGetValue(category.ParentCategoryId.Value, out var parent))
                {
                    if (!parent.Subcategories.Any(s => s.Id == category.Id))
                    {
                        parent.Subcategories.Add(category);
                    }
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
            logger.LogError(ex, "Error querying product category tree.");
            return Result.Failure<List<ProductCategoryTreeDto>>("خطای سیستمی هنگام دریافت لیست دسته‌بندی‌ها رخ داد.");
        }
    }
}