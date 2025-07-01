namespace StoreManagement.WPF.Converters;

public class IsNotPaidConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is InstallmentStatus status)
        {
            // دکمه پرداخت فقط زمانی فعال است که وضعیت قسط "پرداخت شده" نباشد
            return status != InstallmentStatus.Paid;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}