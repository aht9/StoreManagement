namespace StoreManagement.Application.DTOs.Inventories;

public class InventoryTransactionHistoryDto
{
    public DateTime TransactionDate { get; set; }
    public string TransactionType { get; set; }
    public int QuantityChange { get; set; } 
    public string ReferenceInvoiceNumber { get; set; }
    public string Description { get; set; }
    public bool IsIncrease => QuantityChange > 0;
}