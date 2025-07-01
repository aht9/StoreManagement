using StoreManagement.Domain.Events.Inventories;

namespace StoreManagement.Domain.Aggregates.Inventory;

public class InventoryTransaction : BaseEntity, IAggregateRoot
{
    public long ProductVariantId { get; private set; }
    public ProductVariant ProductVariant { get; private set; }

    public DateTime TransactionDate { get; private set; }

    public int Quantity { get; private set; }

    public decimal? PurchasePrice { get; private set; }

    public decimal? SalePrice { get; private set; }

    public string? Description { get; private set; }

    // This property is used for EF Core mapping to the backing field _transactionTypeId
    [NotMapped]
    public InventoryTransactionType TransactionType
    {
        get => InventoryTransactionType.FromValue<InventoryTransactionType>(_transactionTypeId);
        private set => _transactionTypeId = value.Id;
    }
    private int _transactionTypeId;


    public long? ReferenceInvoiceId { get; private set; }
    public InvoiceType? ReferenceInvoiceType { get; private set; }

    protected InventoryTransaction() { }

    public InventoryTransaction(
        long productVariantId,
        ProductVariant productVariant,
        DateTime transactionDate,
        int quantity,
        int transactionTypeId,
        long? referenceInvoiceId = null,
        InvoiceType? referenceInvoiceType = null)
    {
        ProductVariantId = productVariantId;
        ProductVariant = productVariant ?? throw new ArgumentNullException(nameof(productVariant));
        TransactionDate = transactionDate;
        Quantity = quantity;
        _transactionTypeId = transactionTypeId;
        ReferenceInvoiceId = referenceInvoiceId;
        ReferenceInvoiceType = referenceInvoiceType;

        Validate();

        // پس از ساخت موفقیت‌آمیز، رویداد مربوطه را به لیست رویدادها اضافه کن
        this.AddDomainEvent(new InventoryTransactionCreatedEvent(this));
    }

    public void UpdateTransactionDate(DateTime newDate)
    {
        TransactionDate = newDate;
        UpdateTimestamp();
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(newQuantity));

        Quantity = newQuantity;
        UpdateTimestamp();
    }

    public void UpdateTransactionType(InventoryTransactionType newTransactionType)
    {
        _transactionTypeId = newTransactionType.Id;
        UpdateTimestamp();
    }

    public void UpdateReferenceInvoice(long? newInvoiceId, InvoiceType? newInvoiceType)
    {
        ReferenceInvoiceId = newInvoiceId;
        ReferenceInvoiceType = newInvoiceType;
        UpdateTimestamp();
    }

    private void Validate()
    {
        if (ProductVariant == null)
            throw new InvalidOperationException("ProductVariant cannot be null.");

        if (TransactionType == null)
            throw new InvalidOperationException("TransactionType cannot be null.");

        if (Quantity <= 0)
            throw new InvalidOperationException("Quantity must be greater than zero.");
    }

    public void SetPrices(decimal? itemDtoUnitPrice, decimal? salePrice)
    {
        if (itemDtoUnitPrice <= 0)
            throw new ArgumentException("Unit price must be greater than zero.", nameof(itemDtoUnitPrice));
        if (salePrice < 0)
            throw new ArgumentException("Sale price cannot be negative.", nameof(salePrice));
        PurchasePrice = itemDtoUnitPrice;
        SalePrice = salePrice;
        UpdateTimestamp();
    }

    public void SetDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null or empty.", nameof(description));
        Description = description;
        UpdateTimestamp();
    }
}