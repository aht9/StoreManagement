namespace StoreManagement.Domain.Aggregates.Invoices;

public class PurchaseInvoiceItem : InvoiceItem  
{
    public long PurchaseInvoiceId { get; private set; }
    protected PurchaseInvoice PurchaseInvoice { get; private set; }

    private PurchaseInvoiceItem() { }

    public PurchaseInvoiceItem(long purchaseInvoiceId, long productVariantId,
        int quantity, decimal unitPrice, int discount, int tax) : base(productVariantId, quantity, unitPrice, discount, tax)
    {
        if (purchaseInvoiceId <= 0)
            throw new ArgumentException("purchaseInvoiceId must be greater than zero.", nameof(purchaseInvoiceId));
        PurchaseInvoiceId = purchaseInvoiceId;
    }

}
