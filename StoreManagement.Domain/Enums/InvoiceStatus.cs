namespace StoreManagement.Domain.Enums;

public enum InvoiceStatus
{
    [Description("پیش‌ فاکتور")]
    Draft,
    [Description("در انتظار پرداخت")]
    Pending,    
    [Description("پرداخت شده")]
    Paid,        
    [Description("لغو شده")]
    Cancelled   
}