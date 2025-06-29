namespace StoreManagement.Application.DTOs.Products;

public class ProductSearchResultDto
{
    public long ProductId { get; set; }
    public long VariantId { get; set; }
    public string Name { get; set; } // نام محصول اصلی
    public string Sku { get; set; }
    public string Color { get; set; } 
    public string Size { get; set; } 
    public int Stock { get; set; }
    public decimal? LastSalePrice { get; set; }

    public string DisplayVariantName
    {
        get
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(Color))
            {
                parts.Add($"رنگ: {Color}");
            }
            if (!string.IsNullOrWhiteSpace(Size))
            {
                parts.Add($"سایز: {Size}");
            }
            return string.Join(", ", parts);
        }
    }
}