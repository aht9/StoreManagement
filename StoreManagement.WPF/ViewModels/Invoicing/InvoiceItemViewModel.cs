namespace StoreManagement.WPF.ViewModels.Invoicing;

public partial class InvoiceItemViewModel : ObservableObject
{
    public long ProductVariantId { get; set; }
    public string ProductName { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPrice))]
    private int _quantity;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPrice))]
    private decimal _unitPrice;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPrice))]
    private int _discountPercentage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPrice))]
    private int _taxPercentage;

    /// <summary>
    /// این پراپرتی فقط برای فاکتورهای خرید استفاده می‌شود تا قیمت فروش آتی کالا مشخص شود.
    /// </summary>
    public decimal? SalePriceForPurchase { get; set; }


    /// <summary>
    /// این پراپرتی فقط خواندنی، مبلغ نهایی این سطر را محاسبه می‌کند.
    /// فرمول: ابتدا تخفیف اعمال شده، سپس مالیات به نتیجه اضافه می‌شود.
    /// </summary>
    public decimal TotalPrice
    {
        get
        {
            decimal priceAfterDiscount = UnitPrice * (1 - (DiscountPercentage / 100m));
            decimal priceAfterTax = priceAfterDiscount * (1 + (TaxPercentage / 100m));
            return priceAfterTax * Quantity;
        }
    }
}