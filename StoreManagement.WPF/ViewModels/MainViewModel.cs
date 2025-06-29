using StoreManagement.WPF.ViewModels.Invoicing;

namespace StoreManagement.WPF.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IInvoicingViewModelFactory _invoicingViewModelFactory;
    public ISnackbarMessageQueue SnackbarMessageQueue { get; }


    [ObservableProperty]
    private ViewModelBase _currentViewModel;

    public MainViewModel(IServiceProvider serviceProvider, IInvoicingViewModelFactory invoicingViewModelFactory)
    {
        SnackbarMessageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(3));
        _serviceProvider = serviceProvider;
        _invoicingViewModelFactory = invoicingViewModelFactory;
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

    [RelayCommand]
    private void NavigateToSalesInvoice()
    {
        // از کارخانه برای ساخت ViewModel با پارامتر صحیح استفاده می‌کنیم
        var viewModel = _invoicingViewModelFactory.Create(InvoiceType.Sales, this.SnackbarMessageQueue);

        // در اینجا منطق ناوبری به View مربوطه قرار می‌گیرد
        CurrentViewModel = viewModel; 
    }

    // یک کامند جدا برای فاکتور خرید
    [RelayCommand]
    private void NavigateToPurchaseInvoice()
    {
        var viewModel = _invoicingViewModelFactory.Create(InvoiceType.Purchase, this.SnackbarMessageQueue);
        CurrentViewModel = viewModel;
    }
}