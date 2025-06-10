namespace StoreManagement.Application.Features.Customers.Query;

public class GetCustomerByIdQuery : IRequest<Result<CustomerDto>>
{
    public long Id { get; set; }
}


public class GetCustomerByIdQueryHandler(IDapperRepository dapperRepository, ILogger<GetCustomerByIdQueryHandler> logger)
    : IRequestHandler<GetCustomerByIdQuery, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            const string sql = "SELECT Id, FirstName, LastName, PhoneNumber, Email, City, FullAddress, DateOfBirth, NationalCode, CreatedAt FROM Customers WHERE Id = @Id AND IsDeleted = 0";

            var customer = await dapperRepository.QuerySingleOrDefaultAsync<CustomerDto>(sql, new { request.Id }, cancellationToken: cancellationToken);

            if (customer == null)
            {
                return Result.Failure<CustomerDto>("مشتری مورد نظر یافت نشد.");
            }

            return Result.Success(customer);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while fetching customer with ID {CustomerId} using Dapper.", request.Id);
            return Result.Failure<CustomerDto>($"خطای سیستمی در هنگام دریافت اطلاعات مشتری رخ داد: {ex.Message}");
        }
    }
}