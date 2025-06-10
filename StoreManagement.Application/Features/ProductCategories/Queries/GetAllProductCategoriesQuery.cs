namespace StoreManagement.Application.Features.ProductCategories.Queries;

public class GetAllProductCategoriesQuery : IRequest<Result<List<ProductCategoryDto>>>
{
}

public class GetAllProductCategoriesQueryHandler : IRequestHandler<GetAllProductCategoriesQuery, Result<List<ProductCategoryDto>>>
{
    private readonly IDapperRepository _dapper;
    private readonly ILogger<GetAllProductCategoriesQueryHandler> _logger;

    public GetAllProductCategoriesQueryHandler(IDapperRepository dapper, ILogger<GetAllProductCategoriesQueryHandler> logger)
    {
        _dapper = dapper;
        _logger = logger;
    }

    public async Task<Result<List<ProductCategoryDto>>> Handle(GetAllProductCategoriesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            const string sql = @"
                SELECT Id, Name, Description, [Order]
                FROM ProductCategories
                WHERE IsDeleted = 0
                ORDER BY [Order], Name";
            var result = await _dapper.QueryAsync<ProductCategoryDto>(sql, cancellationToken: cancellationToken);
            return Result.Success(result.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying all product categories.");
            return Result.Failure<List<ProductCategoryDto>>("خطای سیستمی هنگام دریافت لیست دسته‌بندی‌ها رخ داد.");
        }
    }
}