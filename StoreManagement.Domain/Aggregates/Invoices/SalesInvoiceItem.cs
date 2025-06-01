namespace StoreManagement.Domain.Aggregates.Invoices;

public class SalesInvoiceItem : InvoiceItem
{
    public long SalesInvoiceId { get; private set; }
    public SalesInvoice SalesInvoice { get; private set; }
    private SalesInvoiceItem() { }
    public SalesInvoiceItem(long salesInvoiceId, long productVariantId,
        int quantity, decimal unitPrice, int discount, int tax) : base(productVariantId, quantity, unitPrice, discount, tax)
    {
        if (salesInvoiceId <= 0)
            throw new ArgumentException("salesInvoiceId must be greater than zero.", nameof(salesInvoiceId));
        SalesInvoiceId = salesInvoiceId;
    }
}