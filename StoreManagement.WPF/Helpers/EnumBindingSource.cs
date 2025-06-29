namespace StoreManagement.WPF.Helpers;

public class EnumBindingSource : MarkupExtension
{
    public Type EnumType { get; private set; }

    public EnumBindingSource(Type enumType)
    {
        if (enumType == null || !enumType.IsEnum)
            throw new ArgumentException("EnumType must not be null and of type Enum");
        EnumType = enumType;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return Enum.GetValues(EnumType)
            .Cast<object>()
            .Select(e => new { Value = e, Description = e.ToString() });
    }
}