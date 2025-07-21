namespace StoreManagement.Application.DTOs.Inventories;

public class InventoryStatusDto
{
    public long ProductVariantId { get; set; }
    public string ProductName { get; set; }
    public string Sku { get; set; }
    public string Color { get; set; }
    public string Size { get; set; }
    public int QuantityOnHand { get; set; } 
    public string CategoryName { get; set; }

    public string DisplayVariantName
    {
        get
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(Color)) parts.Add($"Color: {Color}");
            if (!string.IsNullOrWhiteSpace(Size)) parts.Add($"Size: {Size}");
            return string.Join(", ", parts);
        }
    }
}