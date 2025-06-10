namespace StoreManagement.Application.Features.BankAccounts.Query;

public class GetAllBankAccountsQuery : IRequest<Result<List<BankAccountDto>>> { }

public class GetAllBankAccountsQueryHandler(IDapperRepository dapper, ILogger<GetAllBankAccountsQueryHandler> logger)
    : IRequestHandler<GetAllBankAccountsQuery, Result<List<BankAccountDto>>>
{
    public async Task<Result<List<BankAccountDto>>> Handle(GetAllBankAccountsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            const string sql = "SELECT Id, AccountName, BankName, AccountNumber, CardNumberLastFour, Balance FROM BankAccounts WHERE IsDeleted = 0 ORDER BY AccountName";
            var result = await dapper.QueryAsync<BankAccountDto>(sql, cancellationToken: cancellationToken);
            return Result.Success(result.ToList());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while querying all bank accounts.");
            return Result.Failure<List<BankAccountDto>>("خطای سیستمی هنگام دریافت لیست حساب‌های بانکی رخ داد.");
        }
    }
}