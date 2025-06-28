namespace StoreManagement.WPF.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private ViewModelBase _currentViewModel;

    public MainViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;        
        CurrentViewModel = _serviceProvider.GetRequiredService<DashboardViewModel>();
    }

    [RelayCommand]
    private void NavigateToDashboard()
    {
        CurrentViewModel = _serviceProvider.GetRequiredService<DashboardViewModel>();
    }

    [RelayCommand]
    private void NavigateToCustomers()
    {
        CurrentViewModel = _serviceProvider.GetRequiredService<CustomerManagementViewModel>();
    }


    [RelayCommand]
    private void NavigateToBankAccounts()
    {
        CurrentViewModel = _serviceProvider.GetRequiredService<BankAccountManagementViewModel>();
    }

    [RelayCommand]
    private void NavigateToProducts()
    {
        CurrentViewModel = _serviceProvider.GetRequiredService<ProductManagementViewModel>();
    }

    [RelayCommand]
    private void NavigateToStores()
    {
        CurrentViewModel = _serviceProvider.GetRequiredService<StoreManagementViewModel>();
    }
}