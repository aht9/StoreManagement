namespace StoreManagement.Domain.Events.Inventories;

public class InventoryTransactionCreatedEvent(InventoryTransaction transaction) : INotification
{
    public InventoryTransaction Transaction { get; } = transaction;
}