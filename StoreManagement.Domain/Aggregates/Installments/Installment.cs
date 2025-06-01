namespace StoreManagement.Domain.Aggregates.Installments;

public class Installment : BaseEntity, IAggregateRoot
{
    public long InvoiceId { get; set; }

    public InvoiceType InvoiceType { get; set; }

    public int InstallmentNumber { get; set; }

    /// <summary>
    /// تاریخ سررسید قسط
    /// </summary>
    public DateTime DueDate { get; private set; }

    /// <summary>
    /// مبلغ قابل پرداخت 
    /// </summary>
    public decimal AmountDue { get; private set; }

    /// <summary>
    /// مبلغ پرداخت شده
    /// </summary>
    public decimal AmountPaid { get; private set; }

    public InstallmentStatus Status { get; private set; }

    public Installment(long invoiceId, InvoiceType invoiceType, int installmentNumber, DateTime dueDate, decimal amountDue,decimal amountPaid)
    {
        InvoiceId = invoiceId;
        InvoiceType = invoiceType;
        InstallmentNumber = installmentNumber;
        DueDate = dueDate;
        AmountDue = amountDue;
        AmountPaid = amountPaid;
        Status = InstallmentStatus.NotDue;
    }

    public void MarkAsPaid(decimal paymentAmount)
    {
        if (paymentAmount <= 0)
            throw new ArgumentException("Payment amount must be greater than zero.");

        AmountPaid += paymentAmount;

        if (AmountPaid >= AmountDue)
        {
            Status = InstallmentStatus.Paid;
        }
        else
        {
            Status = InstallmentStatus.Debit;
        }
    }

    public void MarkAsOverdue()
    {
        if (Status == InstallmentStatus.Paid)
            throw new InvalidOperationException("Cannot mark a paid installment as overdue.");

        Status = InstallmentStatus.Overdue;
    }

    public void UpdateDueDate(DateTime newDueDate)
    {
        if (newDueDate <= DateTime.Now)
            throw new ArgumentException("New due date must be in the future.");

        DueDate = newDueDate;
    }

    public void UpdateAmountDue(decimal newAmountDue)
    {
        if (newAmountDue <= 0)
            throw new ArgumentException("Amount due must be greater than zero.");

        AmountDue = newAmountDue;

        if (AmountPaid >= AmountDue)
        {
            Status = InstallmentStatus.Paid;
        }
        else if (AmountPaid > 0)
        {
            Status = InstallmentStatus.Debit;
        }
        else
        {
            Status = InstallmentStatus.NotDue;
        }
    }
}