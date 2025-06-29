namespace StoreManagement.WPF.Converters;

[ValueConversion(typeof(bool), typeof(Visibility))]
public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool flag = false;
        if (value is bool b)
        {
            flag = b;
        }
        else if (value is bool?)
        {
            bool? nullable = (bool?)value;
            flag = nullable.GetValueOrDefault();
        }
        return (flag ? Visibility.Visible : Visibility.Collapsed);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible;
        }
        return false;
    }
}