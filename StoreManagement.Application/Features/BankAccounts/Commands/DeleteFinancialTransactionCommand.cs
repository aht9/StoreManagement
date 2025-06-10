namespace StoreManagement.Application.Features.BankAccounts.Commands;

public class DeleteFinancialTransactionCommand : IRequest<Result>
{
    public long BankAccountId { get; set; }
    public long TransactionId { get; set; }
}


public class DeleteFinancialTransactionCommandHandler(
    IGenericRepository<BankAccount> bankAccountRepo,
    IGenericRepository<FinancialTransaction> transactionRepo,
    IUnitOfWork unitOfWork,
    ILogger<DeleteFinancialTransactionCommandHandler> logger)
    : IRequestHandler<DeleteFinancialTransactionCommand, Result>
{
    public async Task<Result> Handle(DeleteFinancialTransactionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var bankAccount = await bankAccountRepo.GetByIdAsync(request.BankAccountId, cancellationToken);
            if (bankAccount == null) return Result.Failure("حساب بانکی یافت نشد.");

            var transaction = await transactionRepo.GetByIdAsync(request.TransactionId, cancellationToken);
            if (transaction == null) return Result.Failure("تراکنش مورد نظر یافت نشد.");

            // Use domain logic to remove the transaction and update the balance
            bankAccount.RemoveTransaction(transaction);

            // EF Core will automatically detect the removal from the collection
            // when tracking the BankAccount aggregate.
            await bankAccountRepo.UpdateAsync(bankAccount, cancellationToken);
            await transactionRepo.DeleteAsync(transaction, cancellationToken); // Explicitly delete the transaction

            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Transaction {TransactionId} was deleted from account {BankAccountId}.", request.TransactionId, request.BankAccountId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting transaction {TransactionId} from account {BankAccountId}.", request.TransactionId, request.BankAccountId);
            return Result.Failure("خطا در حذف تراکنش.");
        }
    }
}