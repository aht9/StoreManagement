namespace StoreManagement.Application.Features.ProductCategories.Commands;

public class UpdateProductCategoryCommand : IRequest<Result>
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Order { get; set; }
}

public class UpdateProductCategoryCommandHandler : IRequestHandler<UpdateProductCategoryCommand, Result>
{
    private readonly IGenericRepository<ProductCategory> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateProductCategoryCommandHandler> _logger;

    public UpdateProductCategoryCommandHandler(IGenericRepository<ProductCategory> repository, IUnitOfWork unitOfWork, ILogger<UpdateProductCategoryCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateProductCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var category = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (category == null || category.IsDeleted)
            {
                return Result.Failure("دسته‌بندی مورد نظر یافت نشد.");
            }

            var spec = new CustomExpressionSpecification<ProductCategory>(c => c.Name == request.Name && c.Id != request.Id && !c.IsDeleted);
            if (await _repository.AnyAsync(spec, cancellationToken))
            {
                return Result.Failure("دسته‌بندی دیگری با این نام وجود دارد.");
            }

            category.Update(request.Name, request.Description, request.Order);
            await _repository.UpdateAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Product Category with ID {CategoryId} updated successfully.", category.Id);
            return Result.Success("دسته‌بندی با موفقیت به‌روزرسانی شد.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product category with ID {Id}", request.Id);
            return Result.Failure($"خطای سیستمی هنگام به‌روزرسانی دسته‌بندی رخ داد: {ex.Message}");
        }
    }
}