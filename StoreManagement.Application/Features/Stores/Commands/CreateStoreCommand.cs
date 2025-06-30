namespace StoreManagement.Application.Features.Stores.Commands;

public record CreateStoreCommand : IRequest<Result<long>>
{
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? ManagerName { get; set; }
    public string? ContactNumber { get; set; }
    public string? Email { get; set; }

    public string Phone_Number { get; set; } = string.Empty;

    public string Address_City { get; set; } = string.Empty;
    public string Address_FullAddress { get; set; } = string.Empty;


    public class CreateStoreCommandHandler(
        IGenericRepository<Store> storeRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateStoreCommandHandler> logger)
        : IRequestHandler<CreateStoreCommand, Result<long>>
    {
        public async Task<Result<long>> Handle(CreateStoreCommand request, CancellationToken cancellationToken)
        {
            try
            {

                // بررسی اینکه آیا فروشگاهی با این نام از قبل موجود است
                var spec = new CustomExpressionSpecification<Store>(s => s.Name == request.Name && !s.IsDeleted);
                if (await storeRepository.AnyAsync(spec, cancellationToken))
                {
                    throw new InvalidOperationException("فروشگاهی با این نام از قبل در سیستم ثبت شده است.");
                }

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
}
