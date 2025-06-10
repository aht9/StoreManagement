namespace StoreManagement.Application.Features.BankAccounts.Commands;

public class CreateFinancialTransactionCommand : IRequest<Result>
{
    public long BankAccountId { get; set; }
    public decimal Amount { get; set; }
    public TransactionType TransactionType { get; set; }
    public string Description { get; set; }
    public long? InvoiceId { get; set; }
    public InvoiceType? InvoiceType { get; set; }
}


public class CreateFinancialTransactionCommandHandler(
    IGenericRepository<BankAccount> bankAccountRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateFinancialTransactionCommandHandler> logger)
    : IRequestHandler<CreateFinancialTransactionCommand, Result>
{
    public async Task<Result> Handle(CreateFinancialTransactionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var bankAccount = await bankAccountRepository.GetByIdAsync(request.BankAccountId, cancellationToken);
            if (bankAccount == null)
            {
                logger.LogWarning("Attempted to add transaction to a non-existent bank account with ID {BankAccountId}", request.BankAccountId);
                return Result.Failure("حساب بانکی مورد نظر یافت نشد.");
            }

            var transaction = new FinancialTransaction(bankAccount, request.Amount, request.TransactionType, request.Description, request.InvoiceId, request.InvoiceType);

            bankAccount.AddTransaction(transaction);


            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Transaction of {Amount} for account {BankAccountId} created successfully.", request.Amount, request.BankAccountId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating transaction for account {BankAccountId}", request.BankAccountId);
            return Result.Failure($"خطا در ثبت تراکنش: {ex.Message}");
        }
    }
}