using StoreManagement.Domain.Common;

namespace StoreManagement.Application.Features.Stores.Queries;
public record GetAllStoresQuery : IRequest<Result<List<StoreDto>>>
{
    public string? SearchText { get; set; }
}


public class GetAllStoresQueryHandler(IDapperRepository dapperRepository, ILogger<GetAllStoresQueryHandler> logger)
    : IRequestHandler<GetAllStoresQuery, Result<List<StoreDto>>>
{
    public async Task<Result<List<StoreDto>>> Handle(GetAllStoresQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var sqlBuilder = new System.Text.StringBuilder(@"
                SELECT
                    Id, Name, Location, ManagerName, ContactNumber, Email,
                    PhoneNumber AS Phone_Number,
                    Address_City, Address_FullAddress
                FROM Stores
                WHERE IsDeleted = 0");

            var parameters = new DynamicParameters();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                sqlBuilder.Append(@"AND 
(Name LIKE @SearchText OR 
Location LIKE @SearchText OR 
ManagerName LIKE @SearchText OR 
ContactNumber LIKE @SearchText OR 
Email LIKE @SearchText OR 
PhoneNumber LIKE @SearchText OR
Address_City LIKE @SearchText OR 
Address_FullAddress LIKE @SearchText)");
                parameters.Add("SearchText", $"%{request.SearchText}%");
            }

            sqlBuilder.Append(" ORDER BY Name");

            var stores = await dapperRepository.QueryAsync<StoreDto>(sqlBuilder.ToString(), parameters, cancellationToken: cancellationToken);
            return Result.Success(stores.ToList());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving stores using Dapper.");
            return Result.Failure<List<StoreDto>>("خطای سیستمی هنگام دریافت لیست محصولات رخ داد.");
        }
    }
}