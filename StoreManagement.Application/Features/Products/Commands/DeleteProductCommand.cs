namespace StoreManagement.Application.Features.Products.Commands;

public class DeleteProductCommand : IRequest<Result>
{
    public long Id { get; set; }
}

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result>
{
    private readonly IGenericRepository<Product> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteProductCommandHandler> _logger;

    public DeleteProductCommandHandler(IGenericRepository<Product> repository, IUnitOfWork unitOfWork, ILogger<DeleteProductCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (product == null || product.IsDeleted)
            {
                return Result.Failure("محصول مورد نظر یافت نشد.");
            }

            // Check if the product is associated with any invoices
            var isProductInUse = await _repository.AnyAsync(
                new CustomExpressionSpecification<Product>(p => p.Id == request.Id && p.Variants.Any(v => v.IsDeleted == false)),
                cancellationToken
            );

            if (isProductInUse)
            {
                return Result.Failure("محصول در حال استفاده است و نمی‌توان آن را حذف کرد.");
            }

            product.MarkAsDeleted(); 
            await _repository.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Product with ID {ProductId} deleted successfully.", request.Id);
            return Result.Success("محصول با موفقیت حذف شد.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product with ID {Id}", request.Id);
            return Result.Failure($"خطای سیستمی هنگام حذف محصول رخ داد: {ex.Message}");
        }
    }
}