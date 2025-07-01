namespace StoreManagement.Application.DTOs.Invoices;

public class InvoiceForEditDto
{
    public long Id { get; set; }
    public string InvoiceNumber { get; set; }
    public DateTime InvoiceDate { get; set; }
    public long PartyId { get; set; } // شناسه مشتری یا فروشگاه
    public string PartyName { get; set; }
    public PaymentType PaymentType { get; set; }
    public List<InvoiceItemDto> Items { get; set; } = new List<InvoiceItemDto>();
}