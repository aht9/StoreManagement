namespace StoreManagement.Application.Features.BankAccounts.Query;

public class GetBankAccountDetailsQuery : IRequest<Result<BankAccountDetailsDto>>
{
    public long BankAccountId { get; set; }
}

public class GetBankAccountDetailsQueryHandler(
    IDapperRepository dapper,
    ILogger<GetBankAccountDetailsQueryHandler> logger)
    : IRequestHandler<GetBankAccountDetailsQuery, Result<BankAccountDetailsDto>>
{
    public async Task<Result<BankAccountDetailsDto>> Handle(GetBankAccountDetailsQuery request, CancellationToken cancellationToken)
    {
        const string sql = @"
                SELECT Id, AccountName, BankName, AccountNumber, CardNumberLastFour, Balance
                FROM BankAccounts
                WHERE Id = @BankAccountId AND IsDeleted = 0;

                SELECT Id, Amount, TransactionDate, TransactionType, Description, InvoiceId, InvoiceType
                FROM FinancialTransactions
                WHERE BankAccountId = @BankAccountId AND IsDeleted = 0
                ORDER BY TransactionDate DESC;
            ";

        try
        {
            using (var multi = await dapper.QueryMultipleAsync(sql, new { request.BankAccountId }, cancellationToken: cancellationToken))
            {
                var bankAccountDetails = await multi.ReadSingleOrDefaultAsync<BankAccountDetailsDto>();
                if (bankAccountDetails == null)
                {
                    logger.LogWarning("Bank Account details requested for non-existent ID {BankAccountId}", request.BankAccountId);
                    return Result.Failure<BankAccountDetailsDto>("حساب بانکی مورد نظر یافت نشد.");
                }

                bankAccountDetails.Transactions = (await multi.ReadAsync<FinancialTransactionDto>()).ToList();

                bankAccountDetails.TotalCredit = bankAccountDetails.Transactions
                    .Where(t => t.TransactionType == TransactionType.Credit)
                    .Sum(t => t.Amount);

                bankAccountDetails.TotalDebit = bankAccountDetails.Transactions
                    .Where(t => t.TransactionType == TransactionType.Debit)
                    .Sum(t => t.Amount);

                return Result.Success(bankAccountDetails);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while querying details for bank account ID {BankAccountId}", request.BankAccountId);
            return Result.Failure<BankAccountDetailsDto>("خطای سیستمی هنگام دریافت جزئیات حساب بانکی رخ داد.");
        }
    }
}