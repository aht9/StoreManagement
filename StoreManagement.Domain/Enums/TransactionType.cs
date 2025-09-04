namespace StoreManagement.Domain.Enums;

public enum TransactionType
{
    [Description("واریز")]
    Credit,   // واریز
    [Description("برداشت")]
    Debit     // برداشت
}