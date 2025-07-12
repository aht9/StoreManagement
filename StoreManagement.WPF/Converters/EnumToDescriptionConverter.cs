namespace StoreManagement.WPF.Converters;

public class EnumToDescriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || value == string.Empty) return string.Empty;

        Enum myEnum = (Enum)value;
        var description = GetEnumDescription(myEnum);
        return description;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value; // برای ComboBox نیازی به پیاده‌سازی این بخش نیست
    }

    // متد کمکی برای خواندن Description از Enum
    private string GetEnumDescription(Enum enumObj)
    {
        var fieldInfo = enumObj.GetType().GetField(enumObj.ToString());
        var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

        return attributes.Length > 0 ? attributes[0].Description : enumObj.ToString();
    }
}