using StoreManagement.Domain.Specifications;

namespace StoreManagement.Application.Features.ProductCategories.Commands;

public class DeleteProductCategoryCommand : IRequest<Result>
{
    public long Id { get; set; }
}

public class DeleteProductCategoryCommandHandler : IRequestHandler<DeleteProductCategoryCommand, Result>
{
    private readonly IGenericRepository<ProductCategory> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteProductCategoryCommandHandler> _logger;

    public DeleteProductCategoryCommandHandler(IGenericRepository<ProductCategory> repository, IUnitOfWork unitOfWork, ILogger<DeleteProductCategoryCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteProductCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var category = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (category == null || category.IsDeleted)
            {
                return Result.Failure("دسته‌بندی مورد نظر یافت نشد.");
            }

            // Check if the category is in use by products
            var isCategoryInUse = await _repository.AnyAsync(
                new CustomExpressionSpecification<ProductCategory>(c => c.Id == request.Id && c.Products.Any()),
                cancellationToken
            );

            if (isCategoryInUse)
            {
                return Result.Failure("دسته‌بندی در حال استفاده توسط محصولات است و نمی‌توان آن را حذف کرد.");
            }

            category.MarkAsDeleted();
            await _repository.UpdateAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Product Category with ID {CategoryId} deleted successfully.", request.Id);
            return Result.Success("دسته‌بندی با موفقیت حذف شد.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product category with ID {Id}", request.Id);
            return Result.Failure($"خطای سیستمی هنگام حذف دسته‌بندی رخ داد: {ex.Message}");
        }
    }
}