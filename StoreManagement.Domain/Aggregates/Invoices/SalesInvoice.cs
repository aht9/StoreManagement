namespace StoreManagement.Domain.Aggregates.Invoices;

public class SalesInvoice : Invoice<SalesInvoiceItem>
{
    public long CustomerId { get; private set; }
    public Customer Customer { get; private set; }
    private SalesInvoice() { }
    public SalesInvoice(long customerId, string invoiceNumber,
        DateTime invoiceDate, PaymentType paymentType) : base(invoiceNumber, invoiceDate, paymentType)
    {
        if (customerId <= 0)
            throw new ArgumentException("Customer ID must be greater than zero.", nameof(customerId));

        CustomerId = customerId;
    }

    public void AddSalesItem(long variantId, int qty, decimal unitPrice, int discount, int tax)
    {
        var salesItem = new SalesInvoiceItem(Id, variantId, qty, unitPrice, discount, tax);
        AddItem(salesItem);
    }

    public void UpdateDetailSale(string requestInvoiceNumber, DateTime requestInvoiceDate, long requestCustomerId)
    {
        if (requestCustomerId <= 0)
            throw new ArgumentOutOfRangeException(nameof(requestCustomerId), "Invalid store ID.");
        CustomerId = requestCustomerId;
        UpdateDetails(requestInvoiceNumber, requestInvoiceDate);
    }

    public void ClearItems()
    {
        foreach (var item in Items.ToList())
        {
            RemoveItem(item);
        }
    }
}

