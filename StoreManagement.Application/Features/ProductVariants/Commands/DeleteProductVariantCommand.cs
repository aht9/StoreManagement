namespace StoreManagement.Application.Features.ProductVariants.Commands;

public class DeleteProductVariantCommand : IRequest<Result>
{
    public long Id { get; set; }
}

public class DeleteProductVariantCommandHandler : IRequestHandler<DeleteProductVariantCommand, Result>
{
    private readonly IGenericRepository<ProductVariant> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteProductVariantCommandHandler> _logger;

    public DeleteProductVariantCommandHandler(IGenericRepository<ProductVariant> repository, IUnitOfWork unitOfWork, ILogger<DeleteProductVariantCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteProductVariantCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var variant = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (variant == null || variant.IsDeleted)
            {
                return Result.Failure("ویژگی مورد نظر یافت نشد.");
            }
            variant.MarkAsDeleted();
            await _repository.UpdateAsync(variant, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Product Variant with ID {VariantId} deleted successfully.", request.Id);
            return Result.Success("ویژگی محصول با موفقیت حذف شد.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product variant with ID {Id}", request.Id);
            return Result.Failure($"خطای سیستمی هنگام حذف ویژگی رخ داد: {ex.Message}");
        }
    }
}