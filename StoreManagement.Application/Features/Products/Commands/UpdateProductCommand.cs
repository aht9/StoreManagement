namespace StoreManagement.Application.Features.Products.Commands;

public class UpdateProductCommand : IRequest<Result>
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public long? CategoryId { get; set; }
}

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result>
{
    private readonly IGenericRepository<Product> _productRepository;
    private readonly IGenericRepository<ProductCategory> _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateProductCommandHandler> _logger;

    public UpdateProductCommandHandler(IGenericRepository<Product> productRepository, IGenericRepository<ProductCategory> categoryRepository, IUnitOfWork unitOfWork, ILogger<UpdateProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
            if (product == null || product.IsDeleted)
            {
                return Result.Failure("محصول مورد نظر یافت نشد.");
            }

            if (request.CategoryId.HasValue)
            {
                var category = await _categoryRepository.GetByIdAsync(request.CategoryId.Value, cancellationToken);
                if (category == null || category.IsDeleted)
                {
                    return Result.Failure("دسته‌بندی انتخاب شده معتبر نیست.");
                }
            }

            var spec = new CustomExpressionSpecification<Product>(p => p.Name == request.Name && p.Id != request.Id && !p.IsDeleted);
            if (await _productRepository.AnyAsync(spec, cancellationToken))
            {
                return Result.Failure("محصول دیگری با این نام وجود دارد.");
            }

            product.Update(request.Name, request.Description, request.CategoryId);
            await _productRepository.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Product with ID {ProductId} updated successfully.", product.Id);
            return Result.Success("محصول با موفقیت به‌روزرسانی شد.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product with ID {Id}", request.Id);
            return Result.Failure($"خطای سیستمی هنگام به‌روزرسانی محصول رخ داد: {ex.Message}");
        }
    }
}