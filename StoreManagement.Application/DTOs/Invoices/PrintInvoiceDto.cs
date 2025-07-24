namespace StoreManagement.Application.DTOs.Invoices;

public class PrintInvoiceDto
{
    public string SellerName { get; set; } = "فروشگاه پرفروش";
    public string SellerAddress { get; set; } = "خرم آباد، مینی سیتی";
    public string SellerPhone { get; set; } = "066-12345678";
        
    // Invoice Info
    public string InvoiceNumber { get; set; }
    public DateTime InvoiceDate { get; set; }
        
    // Buyer Info
    public string BuyerName { get; set; }
    public string BuyerAddress { get; set; }
    public string BuyerPhone { get; set; }

    // Items
    public List<PrintInvoiceItemDto> Items { get; set; }

    // Final Values
    public decimal SubTotal { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TotalTax { get; set; }
    public decimal GrandTotal { get; set; }
    public string GrandTotalInWords { get; set; }
}