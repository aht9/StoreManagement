namespace StoreManagement.Application.Features.Customers.Command;

public class UpdateCustomerCommand : IRequest<Result>
{
    public long Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string City { get; set; }
    public string FullAddress { get; set; }
    public long? NationalCode { get; set; }
    public DateTime? DateOfBirth { get; set; }
}


public class UpdateCustomerCommandHandler(
    IGenericRepository<Customer> customerRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateCustomerCommandHandler> logger)
    : IRequestHandler<UpdateCustomerCommand, Result>
{
    private readonly IGenericRepository<Customer> _customerRepository = customerRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<UpdateCustomerCommandHandler> _logger = logger;

    public async Task<Result> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);
            if (customer == null || customer.IsDeleted)
            {
                return Result.Failure("مشتری مورد نظر یافت نشد.");
            }

            // Check if another customer with the new phone number already exists
            var phoneNumberResult = PhoneNumber.Create(request.PhoneNumber);
            if (phoneNumberResult.IsFailure) return Result.Failure(phoneNumberResult.Error);

            var phoneSpec = new CustomExpressionSpecification<Customer>(c => c.Phone.Value == phoneNumberResult.Value.Value && c.Id != request.Id && !c.IsDeleted);
            if (await _customerRepository.AnyAsync(phoneSpec, cancellationToken))
            {
                return Result.Failure($"مشتری دیگری با شماره تلفن '{request.PhoneNumber}' موجود است.");
            }

            customer.UpdateContactInfo(request.Email, request.PhoneNumber);
            customer.UpdateAddress(new Address(request.City, request.FullAddress));
            customer.UpdatePersonalInfo(request.FirstName, request.LastName,request.NationalCode,request.DateOfBirth);

            await _customerRepository.UpdateAsync(customer,cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Customer with ID {CustomerId} updated successfully.", customer.Id);

            return Result.Success();
        }
        catch (ArgumentException argEx)
        {
            _logger.LogWarning("Business rule validation failed while updating customer {CustomerId}: {ErrorMessage}", request.Id, argEx.Message);
            return Result.Failure($"خطای اعتبارسنجی: {argEx.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating customer with ID {CustomerId}.", request.Id);
            return Result.Failure($"خطای سیستمی در هنگام ویرایش مشتری رخ داد: {ex.Message}");
        }
    }
}