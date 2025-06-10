namespace StoreManagement.Application.DTOs.BankAccounts;

public class BankAccountDto
{
    public long Id { get; set; }
    public string AccountName { get; set; }
    public string BankName { get; set; }
    public string AccountNumber { get; set; }
    public string CardNumberLastFour { get; set; }
    public decimal Balance { get; set; }
}