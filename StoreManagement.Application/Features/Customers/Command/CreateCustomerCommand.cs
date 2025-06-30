namespace StoreManagement.Application.Features.Customers.Command;

public class CreateCustomerCommand : IRequest<Result<long>>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string City { get; set; }
    public string FullAddress { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public long? NationalCode { get; set; }
}

public class CreateCustomerCommandHandler(
    IGenericRepository<Customer> customerRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateCustomerCommandHandler> logger)
    : IRequestHandler<CreateCustomerCommand, Result<long>>
{
    private IGenericRepository<Customer> _customerRepository = customerRepository;
    private IUnitOfWork _unitOfWork = unitOfWork;
    private ILogger<CreateCustomerCommandHandler> _logger = logger;


    public async Task<Result<long>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // بررسی اینکه آیا مشتری با این کد ملی از قبل موجود است یا خیر
            if (request.NationalCode.HasValue)
            {
                var spec = new CustomExpressionSpecification<Customer>(c => c.NationalCode == request.NationalCode.Value && !c.IsDeleted);
                if (await _customerRepository.AnyAsync(spec, cancellationToken))
                {
                    throw new InvalidOperationException("مشتری با این کد ملی از قبل در سیستم ثبت شده است.");
                }
            }

            var tempPhoneNumber = PhoneNumber.Create(request.PhoneNumber);
            if (tempPhoneNumber.IsFailure)
            {
                return Result.Failure<long>(tempPhoneNumber.Error);
            }

            var phoneSpec = new CustomExpressionSpecification<Customer>(c => c.Phone.Value == tempPhoneNumber.Value.Value && !c.IsDeleted);
            if (await _customerRepository.AnyAsync(phoneSpec, cancellationToken))
            {
                return Result.Failure<long>($"مشتری با شماره تلفن '{request.PhoneNumber}' از قبل موجود است.");
            }

            var customer = new Customer(
                request.FirstName,
                request.LastName,
                request.Email,
                request.PhoneNumber, 
                request.City,
                request.FullAddress,
                request.DateOfBirth,
                request.NationalCode);

            // Add to repository and save
            await _customerRepository.AddAsync(customer, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Customer with ID {CustomerId} created successfully.", customer.Id);

            return Result<long>.Success(customer.Id);
        }
        catch (ArgumentException argEx)
        {
            _logger.LogWarning("Business rule validation failed while creating customer: {ErrorMessage}", argEx.Message);
            return Result.Failure<long>($"خطای اعتبارسنجی: {argEx.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating a customer.");
            return Result.Failure<long>($"خطای سیستمی در هنگام ثبت مشتری رخ داد: {ex.Message}");
        }
    }
}