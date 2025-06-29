namespace StoreManagement.Application.Features.Inventory.EventHandlers;

// این کلاس مسئول به‌روزرسانی موجودی کل محصول است.
public class InventoryTransactionCreatedEventHandler(
    IGenericRepository<Domain.Aggregates.Inventory.Inventory> inventoryRepository)
    : INotificationHandler<InventoryTransactionCreatedEvent>
{
    public async Task Handle(InventoryTransactionCreatedEvent notification, CancellationToken cancellationToken)
    {
        var transaction = notification.Transaction;
        long productVariantId = transaction.ProductVariantId;

        var inventorySpec = new CustomExpressionSpecification<Domain.Aggregates.Inventory.Inventory>(i => i.ProductVariantId == productVariantId);
        var inventory = await inventoryRepository.FirstOrDefaultAsync(inventorySpec, cancellationToken);

        if (inventory == null)
        {
            // اگر رکوردی برای این محصول وجود نداشت (یعنی این اولین تراکنش برای آن است)،
            // یک رکورد جدید ایجاد می‌کنیم.
            inventory = new Domain.Aggregates.Inventory.Inventory(productVariantId, 0);
            await inventoryRepository.AddAsync(inventory, cancellationToken);
        }

        if (transaction.TransactionType == InventoryTransactionType.In)
        {
            // اگر تراکنش از نوع ورودی (خرید) بود، موجودی را افزایش بده
            inventory.IncreaseStock(transaction.Quantity);
        }
        else if (transaction.TransactionType == InventoryTransactionType.Out)
        {
            // اگر تراکنش از نوع خروجی (فروش) بود، موجودی را کاهش بده
            inventory.DecreaseStock(transaction.Quantity);
        }

        await inventoryRepository.UpdateAsync(inventory, cancellationToken);

    }
}