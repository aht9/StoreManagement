using StoreManagement.Application.Features.Customers.Command;
using StoreManagement.Domain.Aggregates.Customers;

namespace StoreManagement.Application.Features.BankAccounts.Commands;

public class UpdateBankAccountCommand : IRequest<Result>
{
    public long Id { get; set; }
    public string AccountName { get; set; }
    public string BankName { get; set; }
    public string AccountNumber { get; set; }
    public string CardNumberLastFour { get; set; }
}

public class UpdateBankAccountCommandHandler(
    IGenericRepository<BankAccount> repository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateBankAccountCommandHandler> logger)
    : IRequestHandler<UpdateBankAccountCommand, Result>
{
    public async Task<Result> Handle(UpdateBankAccountCommand request, CancellationToken cancellationToken)
    {
        var bankAccount = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (bankAccount == null)
        {
            logger.LogWarning("Attempted to update a non-existent bank account with ID {BankAccountId}", request.Id);
            return Result.Failure("حساب بانکی مورد نظر یافت نشد.");
        }

        bankAccount.UpdateAccountDetails(request.AccountName, request.BankName, request.AccountNumber, request.CardNumberLastFour);
        await repository.UpdateAsync(bankAccount, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Bank Account with ID {BankAccountId} updated successfully.", request.Id);
        return Result.Success();
    }
}