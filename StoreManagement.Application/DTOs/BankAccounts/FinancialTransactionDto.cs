namespace StoreManagement.Application.DTOs.BankAccounts;

public class FinancialTransactionDto
{
    public long Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }
    public TransactionType TransactionType { get; set; }
    public string Description { get; set; }
    public long? InvoiceId { get; set; }
    public InvoiceType? InvoiceType { get; set; }
}