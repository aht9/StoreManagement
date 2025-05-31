namespace StoreManagement.Domain.Events.BankAccounts;

public class FinancialTransactionCreatedEvent(BankAccount bankAccount, TransactionType transactionType, decimal amount)
    : INotification
{
    public BankAccount BankAccount { get; } = bankAccount ?? throw new ArgumentNullException(nameof(bankAccount), "Bank account cannot be null.");
    public TransactionType TransactionType { get; } = transactionType;
    public decimal Amount { get; } = amount > 0 ? amount : throw new ArgumentException("Amount must be greater than zero.", nameof(amount));
}
