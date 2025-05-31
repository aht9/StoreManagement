namespace StoreManagement.Domain.Enums;

public class InventoryTransactionType : Enumeration
{
    public static readonly InventoryTransactionType In = new InventoryTransactionType(1, "In");         // ورود کالا (مثلاً خرید یا مرجوعی)
    public static readonly InventoryTransactionType Out = new InventoryTransactionType(2, "Out");       // خروج کالا (مثلاً فروش)
    public static readonly InventoryTransactionType Adjustment = new InventoryTransactionType(3, "Adjustment"); // تعدیل دستی
    public static readonly InventoryTransactionType Transfer = new InventoryTransactionType(4, "Transfer");    // انتقال بین انبارها

    private InventoryTransactionType(int id, string name) : base(id, name) { }
}