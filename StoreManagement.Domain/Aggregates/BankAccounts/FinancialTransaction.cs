namespace StoreManagement.Domain.Aggregates.BankAccounts;

public class FinancialTransaction : BaseEntity
{
    public long BankAccountId { get; private set; }
    private BankAccount BankAccount { get; set; }
    public decimal Amount { get; private set; }
    public DateTime TransactionDate { get; private set; }
    public TransactionType TransactionType { get; private set; }
    public string Description { get; private set; }

    private FinancialTransaction()
    {
        // Required for EF Core or serialization
    }

    // Constructor
    public FinancialTransaction(BankAccount bankAccount, decimal amount, TransactionType transactionType, string description)
    {
        if (bankAccount == null || bankAccount.Id <= 0)
            throw new InvalidOperationException("Bank account must be persisted and have a valid ID before creating a transaction.");

        if (amount <= 0)
            throw new ArgumentException("Transaction amount must be greater than zero.");

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.");

        BankAccount = bankAccount;
        BankAccountId = bankAccount.Id;
        Amount = amount;
        TransactionType = transactionType;
        Description = description;
        TransactionDate = DateTime.UtcNow;

        AddDomainEvent(new FinancialTransactionCreatedEvent(bankAccount, TransactionType, Amount));
    }

    // Apply transaction to the bank account
    public void ApplyToBankAccount()
    {
        if (BankAccount == null)
            throw new InvalidOperationException("Bank account must be associated with the transaction.");

        AddDomainEvent(new FinancialTransactionAppliedEvent(BankAccount, TransactionType, Amount));
    }
}