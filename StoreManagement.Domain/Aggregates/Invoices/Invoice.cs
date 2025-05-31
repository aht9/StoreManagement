namespace StoreManagement.Domain.Aggregates.Invoices;

public abstract class Invoice<TItem> : BaseEntity, IAggregateRoot
    where TItem : InvoiceItem
{
    public long CustomerId { get; private set; }
    public Customer Customer { get; private set; }

    public string InvoiceNumber { get; private set; }
    public DateTime InvoiceDate { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal PaidAmount { get; private set; }
    public decimal DueAmount => TotalAmount - PaidAmount;
    public PaymentType PaymentType { get; private set; }
    public InvoiceStatus InvoiceStatus { get; private set; }

    private readonly List<TItem> _items = new List<TItem>();
    public IReadOnlyCollection<TItem> Items => _items.AsReadOnly();

    protected Invoice()
    {
    }

    // Constructor
    protected Invoice(long customerId, string invoiceNumber, DateTime invoiceDate, PaymentType paymentType)
    {
        CustomerId = customerId;
        InvoiceNumber = invoiceNumber;
        InvoiceDate = invoiceDate;
        PaymentType = paymentType;
        InvoiceStatus = InvoiceStatus.Draft;
    }

    // Behavior methods
    public void AddItem(TItem item)
    {
        _items.Add(item);
        RecalculateTotalAmount();
    }

    public void RemoveItem(TItem item)
    {
        _items.Remove(item);
        RecalculateTotalAmount();
    }

    public void MarkAsPaid(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Paid amount must be greater than zero.");

        PaidAmount += amount;

        if (PaidAmount >= TotalAmount)
        {
            InvoiceStatus = InvoiceStatus.Paid;
        }
        else
        {
            InvoiceStatus = InvoiceStatus.Pending;
        }
    }

    public void CancelInvoice()
    {
        if (InvoiceStatus == InvoiceStatus.Paid)
            throw new InvalidOperationException("Paid invoices cannot be canceled.");

        InvoiceStatus = InvoiceStatus.Cancelled;
    }

    private void RecalculateTotalAmount()
    {
        TotalAmount = _items.Sum(item => item.TotalPrice);
    }
}