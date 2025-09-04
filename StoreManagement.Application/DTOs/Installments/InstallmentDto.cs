namespace StoreManagement.Application.DTOs.Installments;

public class InstallmentDto
{
    public long Id { get; set; }
    public int InstallmentNumber { get; set; }
    public DateTime DueDate { get; set; }
    public decimal AmountDue { get; set; }
    public decimal AmountPaid { get; set; }
    public InstallmentStatus Status { get; set; }
}