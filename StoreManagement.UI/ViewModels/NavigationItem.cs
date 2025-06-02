namespace StoreManagement.UI.ViewModels;

public class NavigationItem : ViewModelBase
{
    private string _displayName = string.Empty;
    public string DisplayName
    {
        get => _displayName;
        set => SetProperty(ref _displayName, value);
    }

    private Type _viewModelType;
    public Type ViewModelType
    {
        get => _viewModelType;
        set => SetProperty(ref _viewModelType, value);
    }
        
    private PackIconKind _iconKind;
    public PackIconKind IconKind
    {
        get => _iconKind;
        set => SetProperty(ref _iconKind, value);
    }

    public NavigationItem(string displayName, Type viewModelType, PackIconKind iconKind)
    {
        DisplayName = displayName;
        ViewModelType = viewModelType;
        IconKind = iconKind;
    }
}