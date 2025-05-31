namespace StoreManagement.Domain.Aggregates.BankAccounts;

public class BankAccount : BaseEntity, IAggregateRoot
{
    public string AccountName { get; private set; }
    public string BankName { get; private set; }
    public string AccountNumber { get; private set; }
    public string CardNumberLastFour { get; private set; }
    public decimal Balance { get; protected set; }

    private readonly List<FinancialTransaction> _transactions = new List<FinancialTransaction>();
    public IReadOnlyCollection<FinancialTransaction> Transactions => _transactions.AsReadOnly();

    private BankAccount() { } // Required for EF Core or serialization

    // Constructor
    public BankAccount(string accountName, string bankName, string accountNumber, string cardNumberLastFour, decimal initialBalance)
    {
        AccountName = accountName ?? throw new ArgumentNullException(nameof(accountName));
        BankName = bankName ?? throw new ArgumentNullException(nameof(bankName));
        AccountNumber = accountNumber ?? throw new ArgumentNullException(nameof(accountNumber));
        CardNumberLastFour = cardNumberLastFour ?? throw new ArgumentNullException(nameof(cardNumberLastFour));
        Balance = initialBalance;

        if (initialBalance > 0)
        {
            // Initial balance transaction is added after the BankAccount is persisted
            AddDomainEvent(new BankAccountCreatedEvent(this, initialBalance));
        }
    }

    // Update account details
    public void UpdateAccountDetails(string accountName, string bankName, string accountNumber, string cardNumberLastFour)
    {
        AccountName = accountName ?? throw new ArgumentNullException(nameof(accountName));
        BankName = bankName ?? throw new ArgumentNullException(nameof(bankName));
        AccountNumber = accountNumber ?? throw new ArgumentNullException(nameof(accountNumber));
        CardNumberLastFour = cardNumberLastFour ?? throw new ArgumentNullException(nameof(cardNumberLastFour));
    }

    // Add transaction
    public void AddTransaction(FinancialTransaction transaction)
    {
        if (transaction == null) throw new ArgumentNullException(nameof(transaction));
        if (Id <= 0) throw new InvalidOperationException("BankAccount must be persisted before adding transactions.");

        _transactions.Add(transaction);
        transaction.ApplyToBankAccount();
        UpdateBalance(transaction);
    }

    // Remove transaction
    public void RemoveTransaction(FinancialTransaction transaction)
    {
        if (transaction == null) throw new ArgumentNullException(nameof(transaction));
        if (!_transactions.Contains(transaction)) throw new InvalidOperationException("Transaction not found.");
        _transactions.Remove(transaction);
        UpdateBalanceAfterRemoval(transaction);
    }

    // Update balance after transaction
    protected void UpdateBalance(FinancialTransaction transaction)
    {
        Balance += transaction.TransactionType == TransactionType.Credit ? transaction.Amount : -transaction.Amount;
    }

    // Update balance after transaction removal
    protected void UpdateBalanceAfterRemoval(FinancialTransaction transaction)
    {
        Balance -= transaction.TransactionType == TransactionType.Credit ? transaction.Amount : -transaction.Amount;
    }
}