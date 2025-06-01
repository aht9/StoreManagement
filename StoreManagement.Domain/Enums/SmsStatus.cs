namespace StoreManagement.Domain.Enums;

public enum SmsStatus
{
    Pending,        // در انتظار
    Sent,           // ارسال شده
    Delivered,      // تحویل داده شده
    Failed          // ناموفق
}