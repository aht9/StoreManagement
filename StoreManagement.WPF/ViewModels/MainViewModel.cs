namespace StoreManagement.WPF.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IMediator _mediator;

    [ObservableProperty]
    private ViewModelBase _currentViewModel;

    public MainViewModel(IMediator mediator)
    {
        _mediator = mediator;
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
        CurrentViewModel = new CustomerManagementViewModel(_mediator);
    }
}