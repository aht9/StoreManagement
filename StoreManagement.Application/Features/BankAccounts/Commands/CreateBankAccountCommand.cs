using StoreManagement.Domain.Common.Interface;

namespace StoreManagement.Application.Features.BankAccounts.Commands;

public class CreateBankAccountCommand : IRequest<Result<long>>
{
    public string AccountName { get; set; }
    public string BankName { get; set; }
    public string AccountNumber { get; set; }
    public string CardNumberLastFour { get; set; }
    public decimal InitialBalance { get; set; }
}



public class CreateBankAccountCommandHandler(
    IGenericRepository<BankAccount> repository,
    IUnitOfWork unitOfWork,
    ILogger<CreateBankAccountCommandHandler> logger)
    : IRequestHandler<CreateBankAccountCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateBankAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var bankAccount = new BankAccount(request.AccountName, request.BankName, request.AccountNumber, request.CardNumberLastFour, request.InitialBalance);

            var spec = new CustomExpressionSpecification<BankAccount>(b => b.AccountNumber == request.AccountNumber && !b.IsDeleted);
            if (await repository.AnyAsync(spec, cancellationToken))
            {
                return Result.Failure<long>("حساب بانکی با این شماره حساب از قبل موجود است.");
            }

            await repository.AddAsync(bankAccount, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Bank Account with ID {BankAccountId} created successfully.", bankAccount.Id);
            return Result.Success(bankAccount.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating bank account with AccountNumber {AccountNumber}", request.AccountNumber);
            return Result.Failure<long>($"خطای سیستمی هنگام ایجاد حساب بانکی رخ داد: {ex.Message}");
        }
    }
}
