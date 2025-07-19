namespace StoreManagement.Application.Features.Products.Commands;

public class UpdateProductCommand : IRequest<Result>
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public long? CategoryId { get; set; }
}

public class UpdateProductCommandHandler(
    IGenericRepository<Product> productRepository,
    IGenericRepository<ProductCategory> categoryRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateProductCommandHandler> logger)
    : IRequestHandler<UpdateProductCommand, Result>
{
    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);
            if (product == null || product.IsDeleted)
            {
                return Result.Failure("محصول مورد نظر یافت نشد.");
            }

            if (request.CategoryId.HasValue)
            {
                var category = await categoryRepository.GetByIdAsync(request.CategoryId.Value, cancellationToken);
                if (category == null || category.IsDeleted)
                {
                    return Result.Failure("دسته‌بندی انتخاب شده معتبر نیست.");
                }
            }

            var spec = new CustomExpressionSpecification<Product>(p => p.Name == request.Name && p.Id != request.Id && !p.IsDeleted);
            if (await productRepository.AnyAsync(spec, cancellationToken))
            {
                return Result.Failure("محصول دیگری با این نام وجود دارد.");
            }

            product.Update(request.Name, request.Description, request.CategoryId);
            await productRepository.UpdateAsync(product, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Product with ID {ProductId} updated successfully.", product.Id);
            return Result.Success("محصول با موفقیت به‌روزرسانی شد.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating product with ID {Id}", request.Id);
            return Result.Failure($"خطای سیستمی هنگام به‌روزرسانی محصول رخ داد: {ex.Message}");
        }
    }
}