namespace StoreManagement.Application.Features.Stores.Commands;

public record UpdateStoreCommand : IRequest<Result<Unit>>
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? ManagerName { get; set; }
    public string? ContactNumber { get; set; }
    public string? Email { get; set; }

    public string Phone_Number { get; set; } = string.Empty; // This maps to PhoneNumber.Value

    public string Address_City { get; set; } = string.Empty;
    public string Address_FullAddress { get; set; } = string.Empty; // Changed from Address_Full
}


public class UpdateStoreCommandHandler(IGenericRepository<Store> storeRepository, IUnitOfWork unitOfWork,
    ILogger<UpdateStoreCommandHandler> logger)
    : IRequestHandler<UpdateStoreCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(UpdateStoreCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var store = await storeRepository.GetByIdAsync(request.Id, cancellationToken); 
            if (store == null)
            {
                return Result.Failure<Unit>("Store not found.");
            }

            var phoneNumberResult = PhoneNumber.Create(request.Phone_Number);
            if (phoneNumberResult.IsFailure)
            {
                return Result.Failure<Unit>(phoneNumberResult.Error);
            }
            var phoneNumber = phoneNumberResult.Value;

            var address = new Address(request.Address_City, request.Address_FullAddress);

            store.UpdateStoreDetails(
                request.Name,
                request.Location,
                request.ManagerName,
                request.ContactNumber,
                request.Email,
                phoneNumber,
                address
            );

            await storeRepository.UpdateAsync(store, cancellationToken); 
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(Unit.Value);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<Unit>(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error Updating store with name {Name}", request.Name);
            return Result.Failure<Unit>($"An error occurred while updating the store: {ex.Message}");
        }
    }
}