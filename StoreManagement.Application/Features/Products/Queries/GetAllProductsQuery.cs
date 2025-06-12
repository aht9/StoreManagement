namespace StoreManagement.Application.Features.Products.Queries;

public class GetAllProductsQuery : IRequest<Result<List<ProductDto>>>
{
}

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, Result<List<ProductDto>>>
{
    private readonly IDapperRepository _dapper;
    private readonly ILogger<GetAllProductsQueryHandler> _logger;

    public GetAllProductsQueryHandler(IDapperRepository dapper, ILogger<GetAllProductsQueryHandler> logger)
    {
        _dapper = dapper;
        _logger = logger;
    }

    public async Task<Result<List<ProductDto>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            const string sql = @"
                SELECT p.Id, p.Name, p.Description, p.CategoryId, c.Name as CategoryName
                FROM Products p
                LEFT JOIN ProductCategories c ON p.CategoryId = c.Id
                WHERE p.IsDeleted = 0
                ORDER BY p.Name";
            var result = await _dapper.QueryAsync<ProductDto>(sql, cancellationToken: cancellationToken);
            return Result.Success(result.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying all products.");
            return Result.Failure<List<ProductDto>>("خطای سیستمی هنگام دریافت لیست محصولات رخ داد.");
        }
    }
}