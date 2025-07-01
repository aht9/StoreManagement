namespace StoreManagement.Application.DTOs.Invoices;

public class InvoiceListDto
{
    public long Id { get; set; }
    public string InvoiceNumber { get; set; }
    public DateTime InvoiceDate { get; set; }
    public string PartyName { get; set; } // نام مشتری یا فروشگاه
    public decimal TotalAmount { get; set; }
    public string PaymentStatusText { get; set; } // متن وضعیت: "پرداخت کامل" یا "پرداخت نشده"
    public bool IsPaid { get; set; }
}