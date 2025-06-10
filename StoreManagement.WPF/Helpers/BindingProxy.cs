namespace StoreManagement.WPF.Helpers;


/// <summary>
/// A helper class that acts as a proxy to overcome DataContext inheritance issues,
/// especially in DataTemplates or other disconnected visual tree contexts.
/// It inherits from Freezable to allow it to be part of a ResourceDictionary
/// and participate in the logical tree, correctly inheriting the DataContext.
/// </summary>
public class BindingProxy : Freezable
{
    protected override Freezable CreateInstanceCore()
    {
        return new BindingProxy();
    }

    public object Data
    {
        get { return GetValue(DataProperty); }
        set { SetValue(DataProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Data. 
    // This enables animation, styling, binding, etc...
    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(nameof(Data), typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));

}