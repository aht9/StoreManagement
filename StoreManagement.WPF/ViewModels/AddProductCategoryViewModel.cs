namespace StoreManagement.WPF.ViewModels;

public partial class AddProductCategoryViewModel :ViewModelBase
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private int _order = 0;
    [ObservableProperty] private long? _parentCategoryId = 0;


    [RelayCommand()]
    private async Task SaveAsync()
    {

    }

    private bool CanSave() => !string.IsNullOrWhiteSpace(Name) && 
                              !string.IsNullOrWhiteSpace(Description) && 
                              Order >= 0;
    

}