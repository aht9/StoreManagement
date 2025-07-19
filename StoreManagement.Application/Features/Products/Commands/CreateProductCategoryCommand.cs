namespace StoreManagement.Application.Features.Products.Commands;

public class CreateProductCategoryCommand : IRequest<Result<long>>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int Order { get; set; }
    public long? ParentCategoryId { get; set; }
}


public class CreateProductCategoryCommandHandler(
    IGenericRepository<ProductCategory> categoryRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateProductCategoryCommandHandler> logger)
    : IRequestHandler<CreateProductCategoryCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateProductCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var parentCategory = request.ParentCategoryId.HasValue
                ? await categoryRepository.GetByIdAsync(request.ParentCategoryId.Value, cancellationToken)
                : null;
            if (parentCategory?.IsDeleted == true)
            {
                throw new InvalidOperationException("دسته‌بندی والد انتخاب شده معتبر نیست.");
            }


            var existingCategorySpec = new CustomExpressionSpecification<ProductCategory>(
                c => c.Name == request.Name && !c.IsDeleted && (request.ParentCategoryId == null || c.ParentCategoryId == request.ParentCategoryId));


            var category = new ProductCategory(request.Name, request.Description, request.Order, parentCategory);
            await categoryRepository.AddAsync(category, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Product category with ID {CategoryId} created successfully.", category.Id);
            return Result<long>.Success(category.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating product category with name {Name}", request.Name);
            return Result.Failure<long>($"خطای سیستمی هنگام ایجاد دسته‌بندی محصول رخ داد: {ex.Message}");
        }
    }
}