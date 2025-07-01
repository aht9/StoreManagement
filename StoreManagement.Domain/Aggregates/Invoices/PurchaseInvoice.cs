namespace StoreManagement.Domain.Aggregates.Invoices;

public class PurchaseInvoice : Invoice<PurchaseInvoiceItem>
{
    public long StoreId { get; private set; }
    public Store Store { get; private set; }


    private PurchaseInvoice() { }


    public PurchaseInvoice(string invoiceNumber, DateTime invoiceDate,
        PaymentType paymentType, long storeId) : base(invoiceNumber, invoiceDate, paymentType)
    {
        if (storeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(storeId), "Invalid store ID.");

        StoreId = storeId;
    }


    public void AddPurchaseItem(long variantId, int qty, decimal unitPrice,int discount, int tax)
    {
        var purchaseItem = new PurchaseInvoiceItem(Id, variantId, qty, unitPrice, discount, tax);

        AddItem(purchaseItem);
    }

    public void UpdateDetailsPurchase(string requestInvoiceNumber, DateTime requestInvoiceDate, long requestStoreId)
    {
        if (requestStoreId <= 0)
            throw new ArgumentOutOfRangeException(nameof(requestStoreId), "Invalid store ID.");
        StoreId = requestStoreId;
        UpdateDetails(requestInvoiceNumber,requestInvoiceDate);
    }

    public void ClearItems()
    {
        foreach (var item in Items.ToList())
        {
            RemoveItem(item);
        }
    }
}