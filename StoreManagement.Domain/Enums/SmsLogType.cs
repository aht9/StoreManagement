namespace StoreManagement.Domain.Enums;

public enum SmsLogType
{
    RequestSent,           // درخواست ارسال شد
    ResponseReceived,      // پاسخ دریافت شد
    Error,                 // خطا
    StatusUpdate           // به‌روزرسانی وضعیت
}