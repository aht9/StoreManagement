namespace StoreManagement.Application.DTOs.Products;

public class ProductVariantDto
{
    public long Id { get; set; }
    public string SKU { get; set; }
    public string Color { get; set; }
    public string Size { get; set; }
    public long ProductId { get; set; }
}