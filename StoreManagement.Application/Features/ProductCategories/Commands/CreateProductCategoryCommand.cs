namespace StoreManagement.Application.Features.ProductCategories.Commands;

public class CreateProductCategoryCommand : IRequest<Result<long>>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int Order { get; set; }
}

public class CreateProductCategoryCommandHandler : IRequestHandler<CreateProductCategoryCommand, Result<long>>
{
    private readonly IGenericRepository<ProductCategory> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateProductCategoryCommandHandler> _logger;

    public CreateProductCategoryCommandHandler(IGenericRepository<ProductCategory> repository, IUnitOfWork unitOfWork, ILogger<CreateProductCategoryCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<long>> Handle(CreateProductCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var category = new ProductCategory(request.Name, request.Description, request.Order, null);

            var spec = new CustomExpressionSpecification<ProductCategory>(c => c.Name == request.Name && !c.IsDeleted);
            if (await _repository.AnyAsync(spec, cancellationToken))
            {
                return Result.Failure<long>("دسته‌بندی با این نام از قبل موجود است.");
            }

            await _repository.AddAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Product Category with ID {CategoryId} created successfully.", category.Id);
            return Result.Success(category.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product category with name {Name}", request.Name);
            return Result.Failure<long>($"خطای سیستمی هنگام ایجاد دسته‌بندی رخ داد: {ex.Message}");
        }
    }
}