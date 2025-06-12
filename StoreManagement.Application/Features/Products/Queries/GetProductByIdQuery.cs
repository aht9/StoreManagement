namespace StoreManagement.Application.Features.Products.Queries;

public class GetProductByIdQuery : IRequest<Result<ProductDto>>
{
    public long Id { get; set; }
}

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    private readonly IDapperRepository _dapper;
    private readonly ILogger<GetProductByIdQueryHandler> _logger;

    public GetProductByIdQueryHandler(IDapperRepository dapper, ILogger<GetProductByIdQueryHandler> logger)
    {
        _dapper = dapper;
        _logger = logger;
    }

    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            const string sql = @"
                SELECT p.Id, p.Name, p.Description, p.CategoryId, c.Name as CategoryName
                FROM Products p
                LEFT JOIN ProductCategories c ON p.CategoryId = c.Id
                WHERE p.IsDeleted = 0 AND p.Id = @Id";

            var result = await _dapper.QuerySingleOrDefaultAsync<ProductDto>(sql, new { request.Id }, cancellationToken: cancellationToken);

            if (result == null)
            {
                return Result.Failure<ProductDto>("محصول مورد نظر یافت نشد.");
            }

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying product with ID {Id}", request.Id);
            return Result.Failure<ProductDto>("خطای سیستمی هنگام دریافت اطلاعات محصول رخ داد.");
        }
    }
}