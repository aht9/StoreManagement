namespace StoreManagement.Domain.Enums;

public enum PaymentType
{
    [Description("نقدی")]
    Cash, 
    [Description("اقساطی")]
    Installment
}