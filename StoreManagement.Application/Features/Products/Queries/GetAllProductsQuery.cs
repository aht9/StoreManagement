namespace StoreManagement.Application.Features.Products.Queries;

public class GetAllProductsQuery : IRequest<Result<List<ProductDto>>>
{
    public string? SearchText { get; set; }
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
            var sqlBuilder = new System.Text.StringBuilder(@"
                SELECT p.Id, p.Name, p.Description, p.CategoryId, c.Name as CategoryName
                FROM Products p
                LEFT JOIN ProductCategories c ON p.CategoryId = c.Id
                WHERE p.IsDeleted = 0");

            var parameters = new DynamicParameters();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                sqlBuilder.Append(" AND (p.Name LIKE @SearchText OR c.Name LIKE @SearchText OR p.Description LIKE @SearchText)");
                parameters.Add("SearchText", $"%{request.SearchText}%");
            }
            sqlBuilder.Append(" ORDER BY p.Name");

            var result = await _dapper.QueryAsync<ProductDto>(sqlBuilder.ToString(), parameters, cancellationToken: cancellationToken);
            return Result.Success(result.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying all products.");
            return Result.Failure<List<ProductDto>>("خطای سیستمی هنگام دریافت لیست محصولات رخ داد.");
        }
    }
}