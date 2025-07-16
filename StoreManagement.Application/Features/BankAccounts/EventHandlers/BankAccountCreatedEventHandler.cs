using StoreManagement.Domain.Events.BankAccounts;

namespace StoreManagement.Application.Features.BankAccounts.EventHandlers;

public class BankAccountCreatedEventHandler(IGenericRepository<FinancialTransaction> transactionRepository) :INotificationHandler<BankAccountCreatedEvent>
{
    public async Task Handle(BankAccountCreatedEvent notification, CancellationToken cancellationToken)
    {
        var account = notification.BankAccount;
        var initializeBalance = notification.InitialBalance;
        var initialTransaction = new FinancialTransaction(
            account,
            initializeBalance,
            TransactionType.Credit,
            "موجودی اولیه حساب",
            null,
            null
        );

        await transactionRepository.AddAsync(initialTransaction, cancellationToken);

    }
}