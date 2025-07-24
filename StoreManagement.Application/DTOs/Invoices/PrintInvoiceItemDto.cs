namespace StoreManagement.Application.DTOs.Invoices;

public class PrintInvoiceItemDto
{
    public int RowNumber { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int Discount { get; set; } 
    public int Tax { get; set; }      

    
    public decimal GrossPrice => Quantity * UnitPrice;
    public decimal DiscountAmount => GrossPrice * (Discount / 100m);
    public decimal PriceAfterDiscount => GrossPrice - DiscountAmount;
    public decimal TaxAmount => PriceAfterDiscount * (Tax / 100m);
    public decimal TotalPrice => PriceAfterDiscount + TaxAmount;
}

