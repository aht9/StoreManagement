namespace StoreManagement.Application.Features.Products.Queries;

public class GetProductCategoryDetailQuery : IRequest<Result<ProductCategoryDetailDto>>
{
    public long? Id { get; set; }
}


public class GetProductCategoryDetailQueryHandler (IDapperRepository dapper, ILogger<GetProductCategoryDetailQueryHandler> logger)
    : IRequestHandler<GetProductCategoryDetailQuery, Result<ProductCategoryDetailDto>>
{
    public async Task<Result<ProductCategoryDetailDto>> Handle(GetProductCategoryDetailQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Id == null)
        {
            return Result.Failure<ProductCategoryDetailDto>("Category ID cannot be null.");
        }

        try
        {
            var sql = @"
                SELECT Id, Name, Description, [Order], ParentCategoryId
                FROM ProductCategories
                WHERE Id = @Id AND IsDeleted = 0";

            var parameters = new DynamicParameters();
            parameters.Add("Id", request.Id);

            var category = await dapper.QueryFirstOrDefaultAsync<ProductCategoryDetailDto>(sql, parameters);
            if (category == null)
            {
                return Result.Failure<ProductCategoryDetailDto>("Category not found.");
            }

            return Result<ProductCategoryDetailDto>.Success(category);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching product category details for ID {Id}", request.Id);
            return Result.Failure<ProductCategoryDetailDto>($"An error occurred while fetching the category: {ex.Message}");
        }
    }
}