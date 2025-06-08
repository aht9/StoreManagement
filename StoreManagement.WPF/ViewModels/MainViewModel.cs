namespace StoreManagement.WPF.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentViewModel;

    public MainViewModel()
    {
        // Set the initial view
        CurrentViewModel = new DashboardViewModel();
    }

    [RelayCommand]
    private void NavigateToDashboard()
    {
        CurrentViewModel = new DashboardViewModel();
    }

    [RelayCommand]
    private void NavigateToCustomers()
    {
        CurrentViewModel = new CustomerManagementViewModel();
    }
}