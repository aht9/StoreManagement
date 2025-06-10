namespace StoreManagement.Application.Features.Customers.Query;

public class GetAllCustomersQuery : IRequest<Result<List<CustomerDto>>>
{
    public string? SearchText { get; set; }
}

public class GetAllCustomersQueryHandler(
    IDapperRepository dapperRepository,
    ILogger<GetAllCustomersQueryHandler> logger)
    : IRequestHandler<GetAllCustomersQuery, Result<List<CustomerDto>>>
{
    public async Task<Result<List<CustomerDto>>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var sqlBuilder = new System.Text.StringBuilder(
                "SELECT Id, FirstName, LastName, PhoneNumber, Email, Address_City, Address_FullAddress, DateOfBirth, NationalCode, CreatedAt FROM Customers WHERE IsDeleted = 0");

            var parameters = new DynamicParameters();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                sqlBuilder.Append(" AND (FirstName LIKE @SearchText OR LastName LIKE @SearchText OR PhoneNumber LIKE @SearchText OR CAST(NationalCode AS NVARCHAR(20)) LIKE @SearchText)");
                parameters.Add("SearchText", $"%{request.SearchText}%");
            }
            sqlBuilder.Append(" ORDER BY CreatedAt DESC");
            var customers = await dapperRepository.QueryAsync<CustomerDto>(sqlBuilder.ToString(), parameters);
            return Result.Success(customers.ToList());

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching all customers with Dapper.");
            return Result.Failure<List<CustomerDto>>($"خطای سیستمی در هنگام دریافت لیست مشتریان رخ داد: {ex.Message}");
        }
    }
}