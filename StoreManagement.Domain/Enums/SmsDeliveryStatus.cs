namespace StoreManagement.Domain.Enums;

public enum SmsDeliveryStatus
{
    Unknown,     //وضعیت نامشخص
    Sent,        //ارسال شد
    Delivered,   //تحویل شد
    Failed,      //ناموفق
    Rejected      //رد شد
}