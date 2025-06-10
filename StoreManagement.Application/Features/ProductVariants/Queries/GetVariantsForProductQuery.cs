namespace StoreManagement.Application.Features.ProductVariants.Queries;

public class GetVariantsForProductQuery : IRequest<Result<List<ProductVariantDto>>>
{
    public long ProductId { get; set; }
}

public class GetVariantsForProductQueryHandler : IRequestHandler<GetVariantsForProductQuery, Result<List<ProductVariantDto>>>
{
    private readonly IDapperRepository _dapper;
    private readonly ILogger<GetVariantsForProductQueryHandler> _logger;

    public GetVariantsForProductQueryHandler(IDapperRepository dapper, ILogger<GetVariantsForProductQueryHandler> logger)
    {
        _dapper = dapper;
        _logger = logger;
    }

    public async Task<Result<List<ProductVariantDto>>> Handle(GetVariantsForProductQuery request, CancellationToken cancellationToken)
    {
        try
        {
            const string sql = @"
                SELECT Id, SKU, Color, Size, ProductId
                FROM ProductVariants
                WHERE IsDeleted = 0 AND ProductId = @ProductId
                ORDER BY Color, Size";
            var result = await _dapper.QueryAsync<ProductVariantDto>(sql, new { request.ProductId }, cancellationToken: cancellationToken);
            return Result.Success(result.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying variants for product ID {ProductId}.", request.ProductId);
            return Result.Failure<List<ProductVariantDto>>("خطای سیستمی هنگام دریافت لیست ویژگی‌ها رخ داد.");
        }
    }
}