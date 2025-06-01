namespace StoreManagement.Domain.Aggregates.Products;

public class ProductVariant : BaseEntity
{
    /// <summary>
    /// Gets the SKU (Stock Keeping Unit) of the product variant.
    /// </summary>
    public string SKU { get; private set; }
    public string? Color { get; private set; }
    public string? Size { get; private set; }
    public int ProductId { get; private set; }
    public Product Product { get; private set; }

    private ProductVariant() { }

    // Constructor for creating a new ProductVariant
    public ProductVariant(string sku, string? color, string? size, int productId)
    {
        SKU = sku ?? throw new ArgumentNullException(nameof(sku));
        Color = color ?? throw new ArgumentNullException(nameof(color));
        Size = size ?? throw new ArgumentNullException(nameof(size));
        ProductId = productId;
    }

    // Method to update the SKU
    public void UpdateSKU(string newSKU)
    {
        if (string.IsNullOrWhiteSpace(newSKU))
            throw new ArgumentException("SKU cannot be null or empty.", nameof(newSKU));

        SKU = newSKU;
        UpdateTimestamp();
    }

    // Method to update the color
    public void UpdateColor(string newColor)
    {
        if (string.IsNullOrWhiteSpace(newColor))
            throw new ArgumentException("Color cannot be null or empty.", nameof(newColor));

        Color = newColor;
        UpdateTimestamp();
    }

    // Method to update the size
    public void UpdateSize(string newSize)
    {
        if (string.IsNullOrWhiteSpace(newSize))
            throw new ArgumentException("Size cannot be null or empty.", nameof(newSize));

        Size = newSize;
        UpdateTimestamp();
    }

    // Method to validate the entity
    public void Validate()
    {
        ValidateEntity(entity =>
            !string.IsNullOrWhiteSpace(SKU) &&
            !string.IsNullOrWhiteSpace(Color) &&
            !string.IsNullOrWhiteSpace(Size) &&
            Product != null
        );
    }
}