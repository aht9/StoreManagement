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
            var includeSpec = new IncludeSpecification<BankAccount>().Include(c => c.Transactions);
            var spec = new CustomExpressionSpecification<BankAccount>(b => b.Id == request.BankAccountId);
            var finalSpec = spec.And(includeSpec);


            var bankAccount = await bankAccountRepo.FirstOrDefaultAsync(finalSpec, cancellationToken);
            if (bankAccount == null) return Result.Failure("حساب بانکی یافت نشد.");

            var transaction = bankAccount.Transactions.FirstOrDefault(t => t.Id == request.TransactionId && !t.IsDeleted);
            if (transaction == null) return Result.Failure("تراکنش مورد نظر در این حساب یافت نشد یا قبلاً حذف شده است.");

            // Use domain logic to remove the transaction and update the balance
            bankAccount.RemoveTransaction(transaction);
            transaction.MarkAsDeleted();

            // EF Core will automatically detect the removal from the collection
            // when tracking the BankAccount aggregate.
            await bankAccountRepo.UpdateAsync(bankAccount, cancellationToken);
            await transactionRepo.UpdateAsync(transaction, cancellationToken);

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