namespace StoreManagement.Domain.Aggregates.Invoices;

public abstract class InvoiceItem : BaseEntity
{
    public long ProductVariantId { get; private set; }
    public ProductVariant ProductVariant { get; private set; }

    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int Discount { get; private set; } // Discount in percentage (0-100)
    public int Tax { get; private set; } // Tax in percentage (0-100)
    public decimal TotalPrice { get; private set; }
    protected InvoiceItem() { }

    public InvoiceItem(long productVariantId, int quantity, decimal unitPrice, int discount, int tax)
    {
        ProductVariantId = productVariantId;
        SetQuantity(quantity);
        SetUnitPrice(unitPrice);
        SetDiscount(discount);
        SetTax(tax);
        UpdateTotalPrice();
    }

    private void UpdateTotalPrice()
    {
        decimal priceAfterDiscount = UnitPrice * (1 - Discount / 100m);
        decimal priceAfterTax = priceAfterDiscount * (1 + Tax / 100m);
        TotalPrice = priceAfterTax * Quantity;
    }

    public void SetQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        Quantity = quantity;
        UpdateTotalPrice(); 
    }

    public void SetUnitPrice(decimal unitPrice)
    {
        if (unitPrice <= 0)
            throw new ArgumentException("Unit price must be greater than zero.", nameof(unitPrice));
        UnitPrice = unitPrice;
        UpdateTotalPrice();
    }

    public void SetDiscount(int discount)
    {
        if (discount < 0 || discount > 100)
            throw new ArgumentException("Discount must be between 0 and 100.", nameof(discount));
        Discount = discount;
        UpdateTotalPrice();
    }

    public void SetTax(int tax)
    {
        if (tax < 0 || tax > 100)
            throw new ArgumentException("Tax must be between 0 and 100.", nameof(tax));
        Tax = tax;
        UpdateTotalPrice();
    }
}