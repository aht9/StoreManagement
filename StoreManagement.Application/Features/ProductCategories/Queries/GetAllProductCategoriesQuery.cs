namespace StoreManagement.Application.Features.ProductCategories.Queries;

public class GetAllProductCategoriesQuery : IRequest<Result<List<ProductCategoryTreeDto>>>
{
}

public class GetAllProductCategoriesQueryHandler(
    IDapperRepository dapper,
    ILogger<GetAllProductCategoriesQueryHandler> logger)
    : IRequestHandler<GetAllProductCategoriesQuery, Result<List<ProductCategoryTreeDto>>>
{
    public async Task<Result<List<ProductCategoryTreeDto>>> Handle(GetAllProductCategoriesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            const string sql = @"
                SELECT Id, Name, Description, [Order]
                FROM ProductCategories
                WHERE IsDeleted = 0
                ORDER BY [Order], Name";
            var result = await dapper.QueryAsync<ProductCategoryTreeDto>(sql, cancellationToken: cancellationToken);
            return Result.Success(result.ToList());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error querying all product categories.");
            return Result.Failure<List<ProductCategoryTreeDto>>("خطای سیستمی هنگام دریافت لیست دسته‌بندی‌ها رخ داد.");
        }
    }
}