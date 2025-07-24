using Unit = MediatR.Unit;

namespace StoreManagement.Application.Features.Stores.Commands;

public record DeleteStoreCommand : IRequest<Result<Unit>>
{
    public long Id { get; set; }
}


public class DeleteStoreCommandHandler(IGenericRepository<Store> storeRepository, IUnitOfWork unitOfWork,
    ILogger<DeleteStoreCommandHandler> logger)
    : IRequestHandler<DeleteStoreCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DeleteStoreCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var store = await storeRepository.GetByIdAsync(request.Id, cancellationToken);

            if (store == null || store.IsDeleted)
            {
                return Result.Failure<Unit>("فروشگاه مورد نظر یافت نشد.");
            }

            // Check if the store is associated with any products
            var isStoreInUse = await storeRepository.AnyAsync(
                new CustomExpressionSpecification<Store>(s => s.Id == request.Id && s.PurchaseInvoices.Any(p => p.IsDeleted == false)),
                cancellationToken
            );

            if (isStoreInUse)
            {
                return Result.Failure<Unit>("فروشگاه در حال استفاده است و نمی‌توان آن را حذف کرد.");
            }

            store.MarkAsDeleted();
            await storeRepository.UpdateAsync(store, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error Delete store");
            return Result.Failure<Unit>($"An error occurred while deleting the store: {ex.Message}");
        }
    }
}