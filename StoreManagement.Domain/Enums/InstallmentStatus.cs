namespace StoreManagement.Domain.Enums;

public enum InstallmentStatus
{
    [Description("سررسید نشده")]
    NotDue,

    [Description("بدهکار")]
    Debit,

    [Description("پرداخت شده")]
    Paid,

    [Description("معوق")]
    Overdue
}