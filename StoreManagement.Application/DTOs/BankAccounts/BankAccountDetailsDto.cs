namespace StoreManagement.Application.DTOs.BankAccounts;

public class BankAccountDetailsDto : BankAccountDto
{
    public decimal TotalCredit { get; set; }
    public decimal TotalDebit { get; set; }
    public List<FinancialTransactionDto> Transactions { get; set; } = new();
}