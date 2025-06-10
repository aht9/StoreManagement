namespace StoreManagement.Application.Features.ProductVariants.Commands;

public class CreateProductVariantCommand : IRequest<Result<long>>
{
    public long ProductId { get; set; }
    public string SKU { get; set; }
    public string Color { get; set; }
    public string Size { get; set; }
}

public class CreateProductVariantCommandHandler : IRequestHandler<CreateProductVariantCommand, Result<long>>
{
    private readonly IGenericRepository<ProductVariant> _variantRepository;
    private readonly IGenericRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateProductVariantCommandHandler> _logger;

    public CreateProductVariantCommandHandler(IGenericRepository<ProductVariant> variantRepository, IGenericRepository<Product> productRepository, IUnitOfWork unitOfWork, ILogger<CreateProductVariantCommandHandler> logger)
    {
        _variantRepository = variantRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<long>> Handle(CreateProductVariantCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null || product.IsDeleted)
            {
                return Result.Failure<long>("محصول انتخاب شده برای افزودن ویژگی معتبر نیست.");
            }

            var variant = new ProductVariant(request.SKU, request.Color, request.Size, request.ProductId);

            var spec = new CustomExpressionSpecification<ProductVariant>(v => v.SKU == request.SKU && !v.IsDeleted);
            if (await _variantRepository.AnyAsync(spec, cancellationToken))
            {
                return Result.Failure<long>("ویژگی با این SKU از قبل موجود است.");
            }

            await _variantRepository.AddAsync(variant, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Product Variant with ID {VariantId} for product {ProductId} created successfully.", variant.Id, request.ProductId);
            return Result.Success(variant.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product variant for product {ProductId}", request.ProductId);
            return Result.Failure<long>($"خطای سیستمی هنگام ایجاد ویژگی محصول رخ داد: {ex.Message}");
        }
    }
}