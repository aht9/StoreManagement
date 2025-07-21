namespace StoreManagement.WPF.Converters;

public class DecimalFormattingConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal val)
        {
            return val.ToString("N0", culture);
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            if (decimal.TryParse(str.Replace(",", ""), NumberStyles.Any, culture, out decimal result))
            {
                return result;
            }
        }
        return 0m;
    }
}