using StoreManagement.Domain.Aggregates.Products;

namespace StoreManagement.Domain.Aggregates.Inventory;

public class InventoryTransaction : BaseEntity, IAggregateRoot
{
    public long ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; }

    public DateTime TransactionDate { get; set; }

    public int Quantity { get; set; }

    public InventoryTransactionType TransactionType { get; set; } 
    private int _transactionTypeId;



}