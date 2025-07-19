namespace StoreManagement.Application.Features.Products.Commands;

public class CreateProductCommand : IRequest<Result<long>>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public long? CategoryId { get; set; }
}

public class CreateProductCommandHandler(
    IGenericRepository<Product> productRepository,
    IGenericRepository<ProductCategory> categoryRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateProductCommandHandler> logger)
    : IRequestHandler<CreateProductCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.CategoryId.HasValue)
            {
                var category = await categoryRepository.GetByIdAsync(request.CategoryId.Value, cancellationToken);
                if (category == null || category.IsDeleted)
                {
                    return Result.Failure<long>("دسته‌بندی انتخاب شده معتبر نیست.");
                }
            }

            var product = new Product(request.Name, request.Description, request.CategoryId);

            var spec = new CustomExpressionSpecification<Product>(p => p.Name == request.Name && !p.IsDeleted);
            if (await productRepository.AnyAsync(spec, cancellationToken))
            {
                return Result.Failure<long>("محصولی با این نام از قبل موجود است.");
            }

            await productRepository.AddAsync(product, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Product with ID {ProductId} created successfully.", product.Id);
            return Result.Success(product.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating product with name {Name}", request.Name);
            return Result.Failure<long>($"خطای سیستمی هنگام ایجاد محصول رخ داد: {ex.Message}");
        }
    }
}