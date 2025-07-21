namespace StoreManagement.WPF.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IInvoicingViewModelFactory _invoicingViewModelFactory;
    private readonly IInvoiceListViewModelFactory _invoiceListFactory;
    private readonly IEditInvoiceViewModelFactory _editInvoiceFactory;
    private readonly IInstallmentManagementViewModelFactory _installmentManagementFactory;

    public ISnackbarMessageQueue SnackbarMessageQueue { get; }


    [ObservableProperty]
    private ViewModelBase _currentViewModel;

    public MainViewModel(IServiceProvider serviceProvider, 
        IInvoicingViewModelFactory invoicingViewModelFactory,
        IInvoiceListViewModelFactory invoiceListFactory,
        IEditInvoiceViewModelFactory editInvoiceFactory,
        IInstallmentManagementViewModelFactory installmentManagementFactory)
    {
        SnackbarMessageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(3));
        _serviceProvider = serviceProvider;

        _invoicingViewModelFactory = invoicingViewModelFactory;
        _invoiceListFactory = invoiceListFactory;
        _editInvoiceFactory = editInvoiceFactory;

        CurrentViewModel = _serviceProvider.GetRequiredService<DashboardViewModel>();
        _installmentManagementFactory = installmentManagementFactory;
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
        var viewModel = _invoicingViewModelFactory.Create(InvoiceType.Sales, this.SnackbarMessageQueue);
        CurrentViewModel = viewModel; 
    }

    [RelayCommand]
    private void NavigateToPurchaseInvoice()
    {
        var viewModel = _invoicingViewModelFactory.Create(InvoiceType.Purchase, this.SnackbarMessageQueue);
        CurrentViewModel = viewModel;
    }


    [RelayCommand]
    public void NavigateToListSalesInvoices()
    {
        CurrentViewModel = _invoiceListFactory.Create(InvoiceType.Sales, this);
    }

    [RelayCommand]
    public void NavigateToListPurchaseInvoices()
    {
        CurrentViewModel = _invoiceListFactory.Create(InvoiceType.Purchase, this);
    }

    public void NavigateToEditInvoice(long invoiceId, InvoiceType invoiceType)
    {
        CurrentViewModel = _editInvoiceFactory.Create(invoiceId, invoiceType, this);
    }

    public void NavigateToInstallmentManagement(long invoiceId, InvoiceType invoiceType)
    {
        CurrentViewModel = _installmentManagementFactory.Create(invoiceId, invoiceType);
    }

    [RelayCommand]
    public void NavigateToProductCategory()
    {
        CurrentViewModel = _serviceProvider.GetRequiredService<ProductCategoryManagementViewModel>();
    }

    [RelayCommand]
    public void NavigateToInventory()
    {
        CurrentViewModel = _serviceProvider.GetRequiredService<InventoryManagementViewModel>();
    }
    
}