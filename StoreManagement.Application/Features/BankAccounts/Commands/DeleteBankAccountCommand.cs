namespace StoreManagement.Application.Features.BankAccounts.Commands;

public class DeleteBankAccountCommand : IRequest<Result>
{
    public long Id { get; set; }
}


public class DeleteBankAccountCommandHandler(
    IGenericRepository<BankAccount> repository,
    IUnitOfWork unitOfWork,
    ILogger<DeleteBankAccountCommandHandler> logger)
    : IRequestHandler<DeleteBankAccountCommand, Result>
{
    public async Task<Result> Handle(DeleteBankAccountCommand request, CancellationToken cancellationToken)
    {
        var bankAccount = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (bankAccount == null)
        {
            logger.LogWarning("Attempted to delete a non-existent bank account with ID {BankAccountId}", request.Id);
            return Result.Failure("حساب بانکی مورد نظر یافت نشد.");
        }

        if (bankAccount.Balance != 0)
        {
            logger.LogWarning("Attempted to delete bank account {BankAccountId} with a non-zero balance of {Balance}", request.Id, bankAccount.Balance);
            return Result.Failure("امکان حذف حساب بانکی با موجودی غیر صفر وجود ندارد.");
        }

        bankAccount.MarkAsDeleted(); // Soft delete
        await repository.UpdateAsync(bankAccount, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Bank Account with ID {BankAccountId} soft-deleted successfully.", request.Id);
        return Result.Success();
    }
}