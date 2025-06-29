namespace StoreManagement.Domain.Aggregates.Inventory;

public class Inventory : BaseEntity, IAggregateRoot
{
    /// <summary>
    /// شناسه واریانت محصول که این موجودی به آن تعلق دارد.
    /// </summary>
    public long ProductVariantId { get; private set; }

    /// <summary>
    /// تعداد موجودی فعلی در انبار.
    /// </summary>
    public int Quantity { get; private set; }

    // سازنده خصوصی برای استفاده EF Core
    private Inventory() { }

    // سازنده اصلی برای ایجاد یک رکورد موجودی جدید
    public Inventory(long productVariantId, int initialQuantity = 0)
    {
        if (productVariantId <= 0)
            throw new ArgumentOutOfRangeException(nameof(productVariantId), "شناسه واریانت محصول نامعتبر است.");

        ProductVariantId = productVariantId;
        Quantity = initialQuantity;
    }

    /// <summary>
    /// متد برای افزایش موجودی (مثلاً هنگام خرید).
    /// </summary>
    /// <param name="quantityToAdd">تعدادی که باید اضافه شود.</param>
    public void IncreaseStock(int quantityToAdd)
    {
        if (quantityToAdd <= 0)
            throw new ArgumentException("تعداد برای افزایش باید مثبت باشد.", nameof(quantityToAdd));

        Quantity += quantityToAdd;
        UpdateTimestamp();
    }

    /// <summary>
    /// متد برای کاهش موجودی (مثلاً هنگام فروش).
    /// </summary>
    /// <param name="quantityToRemove">تعدادی که باید کسر شود.</param>
    public void DecreaseStock(int quantityToRemove)
    {
        if (quantityToRemove <= 0)
            throw new ArgumentException("تعداد برای کاهش باید مثبت باشد.", nameof(quantityToRemove));

        if (Quantity < quantityToRemove)
            throw new InvalidOperationException("موجودی انبار برای کسر این تعداد کافی نیست.");

        Quantity -= quantityToRemove;
        UpdateTimestamp();
    }
}