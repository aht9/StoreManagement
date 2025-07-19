namespace StoreManagement.Application.Features.Products.Commands;

public class UpdateProductCategoryCommand : IRequest<Result>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int Order { get; set; }
    public long CategoryId { get; set; }
    public long? ParentCategoryId { get; set; }
}

public class UpdateProductCategoryCommandHandler(
    IGenericRepository<ProductCategory> categoryRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateProductCategoryCommandHandler> logger)
    : IRequestHandler<UpdateProductCategoryCommand, Result>
{
    public async Task<Result> Handle(UpdateProductCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var category = await categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category == null || category.IsDeleted)
            {
                return Result.Failure("دسته‌بندی محصول یافت نشد یا حذف شده است.");
            }
            var parentCategory = request.ParentCategoryId.HasValue
                ? await categoryRepository.GetByIdAsync(request.ParentCategoryId.Value, cancellationToken)
                : null;
            if (parentCategory?.IsDeleted == true)
            {
                return Result.Failure("دسته‌بندی والد انتخاب شده معتبر نیست.");
            }

            if (category.Name != request.Name)
            {
                var spec = new CustomExpressionSpecification<ProductCategory>(c => c.Name == request.Name && !c.IsDeleted && c.Id != request.CategoryId);
                if (await categoryRepository.AnyAsync(spec, cancellationToken))
                {
                    return Result.Failure("دسته‌بندی با این نام از قبل وجود دارد.");
                }
                category.UpdateName(request.Name);
            }

            if (category.Description != request.Description)
            {
                category.UpdateDescription(request.Description);
            }

            if (category.Order != request.Order)
            {
                category.UpdateOrder(request.Order);
            }

            if (parentCategory != null)
            {
                category.SetParentCategory(parentCategory);
            }
            await categoryRepository.UpdateAsync(category, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Product category with ID {CategoryId} updated successfully.", category.Id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating product category with ID {CategoryId}", request.CategoryId);
            return Result.Failure($"خطای سیستمی هنگام به‌روزرسانی دسته‌بندی محصول رخ داد: {ex.Message}");
        }
    }
}