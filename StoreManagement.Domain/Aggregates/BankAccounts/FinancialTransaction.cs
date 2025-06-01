namespace StoreManagement.Domain.Aggregates.BankAccounts;

public class FinancialTransaction : BaseEntity
{
    public long BankAccountId { get; private set; }
    public BankAccount BankAccount { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime TransactionDate { get; private set; }
    public TransactionType TransactionType { get; private set; }
    public string? Description { get; private set; }
    public long? InvoiceId { get; set; }
    public InvoiceType? InvoiceType { get; set; }


    private FinancialTransaction()
    {
        // Required for EF Core or serialization
    }

    // Constructor
    public FinancialTransaction(BankAccount bankAccount, 
        decimal amount, TransactionType transactionType, 
        string? description, long? invoiceId, InvoiceType? invoiceType)
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
        InvoiceId = invoiceId;
        InvoiceType = invoiceType;

        AddDomainEvent(new FinancialTransactionCreatedEvent(bankAccount, TransactionType, Amount));
    }

}