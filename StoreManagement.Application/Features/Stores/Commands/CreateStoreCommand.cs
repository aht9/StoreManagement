using StoreManagement.Application.Features.Products.Commands;

namespace StoreManagement.Application.Features.Stores.Commands;

public record CreateStoreCommand : IRequest<Result<long>>
{
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? ManagerName { get; set; }
    public string? ContactNumber { get; set; }
    public string? Email { get; set; }

    public string Phone_Number { get; set; } = string.Empty; // This maps to PhoneNumber.Value

    public string Address_City { get; set; } = string.Empty;
    public string Address_FullAddress { get; set; } = string.Empty; // Changed from Address_Full
}


public class CreateStoreCommandHandler(IGenericRepository<Store> storeRepository, IUnitOfWork unitOfWork,
    ILogger<CreateStoreCommandHandler> logger)
    : IRequestHandler<CreateStoreCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateStoreCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var phoneNumberResult = PhoneNumber.Create(request.Phone_Number);
            if (phoneNumberResult.IsFailure)
            {
                return Result.Failure<long>(phoneNumberResult.Error);
            }
            var phoneNumber = phoneNumberResult.Value;

            var address = new Address(request.Address_City, request.Address_FullAddress);

            var store = new Store(
                request.Name,
                request.Location,
                request.ManagerName,
                request.ContactNumber = phoneNumber.Value,
                request.Email,
                phoneNumber,
                address
            );

            await storeRepository.AddAsync(store, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<long>.Success(store.Id);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating store with name {Name}", request.Name);
            return Result.Failure<long>($"An error occurred while creating the store: {ex.Message}");
        }
    }
}