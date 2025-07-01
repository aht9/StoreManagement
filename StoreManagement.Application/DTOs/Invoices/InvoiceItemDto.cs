namespace StoreManagement.Application.DTOs.Invoices;

public class InvoiceItemDto
{
    public long ProductVariantId { get; set; }
    public string ProductName { get; set; } 
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int DiscountPercentage { get; set; }
    public int TaxPercentage { get; set; }
    public decimal? SalePriceForPurchase { get; set; }
}