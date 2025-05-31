namespace StoreManagement.Domain.Events.BankAccounts;

public class BankAccountCreatedEvent(BankAccount bankAccount, decimal initialBalance) : INotification
{
    public BankAccount BankAccount { get; } = bankAccount ?? throw new ArgumentNullException(nameof(bankAccount), "Bank account cannot be null.");
    public decimal InitialBalance { get; } = initialBalance >= 0 ? initialBalance : throw new ArgumentException("Initial balance must be non-negative.", nameof(initialBalance));
}