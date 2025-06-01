namespace StoreManagement.Domain.Enums;

public enum InvoiceStatus
{
    Draft,       // پیش‌ فاکتور
    Pending,     // در انتظار پرداخت
    Paid,        // پرداخت شده
    Cancelled    // لغو شده  
}