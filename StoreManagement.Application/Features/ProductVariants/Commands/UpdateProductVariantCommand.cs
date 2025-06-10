namespace StoreManagement.Application.Features.ProductVariants.Commands;

public class UpdateProductVariantCommand : IRequest<Result>
{
    public long Id { get; set; }
    public string SKU { get; set; }
    public string Color { get; set; }
    public string Size { get; set; }
}

public class UpdateProductVariantCommandHandler : IRequestHandler<UpdateProductVariantCommand, Result>
{
    private readonly IGenericRepository<ProductVariant> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateProductVariantCommandHandler> _logger;

    public UpdateProductVariantCommandHandler(IGenericRepository<ProductVariant> repository, IUnitOfWork unitOfWork, ILogger<UpdateProductVariantCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateProductVariantCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var variant = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (variant == null || variant.IsDeleted)
            {
                return Result.Failure("ویژگی مورد نظر یافت نشد.");
            }

            var spec = new CustomExpressionSpecification<ProductVariant>(v => v.SKU == request.SKU && v.Id != request.Id && !v.IsDeleted);
            if (await _repository.AnyAsync(spec, cancellationToken))
            {
                return Result.Failure("ویژگی دیگری با این SKU وجود دارد.");
            }

            variant.Update(request.SKU, request.Color, request.Size);
            await _repository.UpdateAsync(variant, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Product Variant with ID {VariantId} updated successfully.", variant.Id);
            return Result.Success("ویژگی محصول با موفقیت به‌روزرسانی شد.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product variant with ID {Id}", request.Id);
            return Result.Failure($"خطای سیستمی هنگام به‌روزرسانی ویژگی رخ داد: {ex.Message}");
        }
    }
}