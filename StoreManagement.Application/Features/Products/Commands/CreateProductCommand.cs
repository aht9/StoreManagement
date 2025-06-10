namespace StoreManagement.Application.Features.Products.Commands;

public class CreateProductCommand : IRequest<Result<long>>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public long? CategoryId { get; set; }
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<long>>
{
    private readonly IGenericRepository<Product> _productRepository;
    private readonly IGenericRepository<ProductCategory> _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateProductCommandHandler> _logger;

    public CreateProductCommandHandler(IGenericRepository<Product> productRepository, IGenericRepository<ProductCategory> categoryRepository, IUnitOfWork unitOfWork, ILogger<CreateProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<long>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.CategoryId.HasValue)
            {
                var category = await _categoryRepository.GetByIdAsync(request.CategoryId.Value, cancellationToken);
                if (category == null || category.IsDeleted)
                {
                    return Result.Failure<long>("دسته‌بندی انتخاب شده معتبر نیست.");
                }
            }

            var product = new Product(request.Name, request.Description, request.CategoryId);

            var spec = new CustomExpressionSpecification<Product>(p => p.Name == request.Name && !p.IsDeleted);
            if (await _productRepository.AnyAsync(spec, cancellationToken))
            {
                return Result.Failure<long>("محصولی با این نام از قبل موجود است.");
            }

            await _productRepository.AddAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Product with ID {ProductId} created successfully.", product.Id);
            return Result.Success(product.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product with name {Name}", request.Name);
            return Result.Failure<long>($"خطای سیستمی هنگام ایجاد محصول رخ داد: {ex.Message}");
        }
    }
}