namespace StoreManagement.Application.DTOs.Invoices;

public class InstallmentDetailsDto
{
    public decimal DownPayment { get; set; } // پیش پرداخت
    public int Months { get; set; }
    public double InterestRate { get; set; } // درصد سود
}