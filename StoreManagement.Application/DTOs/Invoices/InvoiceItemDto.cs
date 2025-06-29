namespace StoreManagement.Application.DTOs.Invoices;

public class InvoiceItemDto
{
    public long ProductVariantId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    // بر اساس کلاس InvoiceItem، تخفیف و مالیات به صورت درصد (int) هستند
    public int DiscountPercentage { get; set; }
    public int TaxPercentage { get; set; }
    public decimal? SalePriceForPurchase { get; set; } // فقط برای فاکتور خرید
}