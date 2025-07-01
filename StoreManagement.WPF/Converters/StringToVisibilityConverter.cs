namespace StoreManagement.WPF.Converters;

public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var stringValue = value as string;

        var targetValue = parameter as string;

        if (stringValue != null && stringValue.Equals(targetValue, StringComparison.OrdinalIgnoreCase))
        {
            return Visibility.Visible;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}