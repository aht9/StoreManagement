namespace StoreManagement.Application.Features.Stores.Queries;

public record GetStoreByIdQuery : IRequest<Result<StoreDto>>
{
    public long Id { get; set; }
}


public class GetStoreByIdQueryHandler(IDapperRepository dapperRepository, ILogger<GetStoreByIdQueryHandler> logger)
    : IRequestHandler<GetStoreByIdQuery, Result<StoreDto>>
{
    public async Task<Result<StoreDto>> Handle(GetStoreByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var sql = @"
                SELECT
                    Id, Name, Location, ManagerName, ContactNumber, Email,
                    PhoneNumber AS Phone_Number,
                    Address_City, Address_FullAddress
                FROM Stores
                WHERE Id = @Id AND IsDeleted = 0";

            var storeDto = await dapperRepository.QueryFirstOrDefaultAsync<StoreDto>(sql, new { request.Id }, cancellationToken: cancellationToken);

            if (storeDto == null)
            {
                return Result.Failure<StoreDto>("Store not found.");
            }

            return Result.Success(storeDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving the store by ID using Dapper."); 
            return Result.Failure<StoreDto>($"An error occurred while retrieving the store: {ex.Message}");
        }
    }
}