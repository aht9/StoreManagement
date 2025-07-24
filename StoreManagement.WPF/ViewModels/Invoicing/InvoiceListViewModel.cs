namespace StoreManagement.WPF.ViewModels.Invoicing;

public partial class InvoiceListViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly MainViewModel _mainViewModel;
    private readonly ISnackbarMessageQueue _snackbarMessageQueue;
    public InvoiceType InvoiceType { get; }
    
    private List<InvoiceListDto> _allInvoices = new();
    [ObservableProperty]
    private ObservableCollection<InvoiceListDto> _pagedInvoices;
    // --- پراپرتی‌های صفحه‌بندی ---
    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(GoToNextPageCommand))] [NotifyCanExecuteChangedFor(nameof(GoToPreviousPageCommand))]
    private int _currentPage = 1;
    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(GoToNextPageCommand))] [NotifyCanExecuteChangedFor(nameof(GoToPreviousPageCommand))]
    private int _pageSize = 10; // می‌توانید این مقدار را تغییر دهید
    public int TotalPages => (_allInvoices == null || !_allInvoices.Any()) ? 1 : (int)Math.Ceiling((double)_allInvoices.Count / _pageSize);

    // پراپرتی‌های مربوط به فیلتر
    [ObservableProperty] private DateTime? _startDate;
    [ObservableProperty] private DateTime? _endDate;
    public IEnumerable<InvoiceStatus> StatusOptions { get; }
    public InvoiceStatus SelectedStatus { get; set; }

    public string PageTitle => InvoiceType == InvoiceType.Sales ? "مدیریت فاکتورهای فروش" : "مدیریت فاکتورهای خرید";

    public InvoiceListViewModel(IMediator mediator, MainViewModel mainViewModel, ISnackbarMessageQueue snackbarMessageQueue, InvoiceType invoiceType)
    {
        _mediator = mediator;
        _mainViewModel = mainViewModel;
        _snackbarMessageQueue = snackbarMessageQueue;
        InvoiceType = invoiceType;
        StatusOptions = Enum.GetValues(typeof(InvoiceStatus)).Cast<InvoiceStatus>();

        Task.Run(LoadInvoices);
    }

    [RelayCommand]
    private async Task LoadInvoices()
    {
        try
        {
            var query = new GetInvoicesQuery
            {
                InvoiceType = InvoiceType,
                StartDate = StartDate,
                EndDate = EndDate,
                Status = this.SelectedStatus
            };
            var result = await _mediator.Send(query);
            _allInvoices = result.ToList();
            CurrentPage = 1; 
            OnPropertyChanged(nameof(TotalPages)); 
            UpdatePagedInvoices();
        }
        catch (Exception ex)
        {
            _snackbarMessageQueue.Enqueue($"خطا در بارگذاری فاکتورها: {ex.Message}");
        }
    }
    
    private void UpdatePagedInvoices()
    {
        var pagedData = _allInvoices
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize);
        PagedInvoices = new ObservableCollection<InvoiceListDto>(pagedData);
    }

    [RelayCommand]
    private void EditInvoice(InvoiceListDto invoice)
    {
        _mainViewModel.NavigateToEditInvoice(invoice.Id, InvoiceType);
    }


    [RelayCommand]
    private async Task DeleteInvoice(InvoiceListDto invoice)
    {
        var dialogViewModel = new ConfirmationDialogViewModel("تایید حذف", $"آیا از حذف فاکتور شماره {invoice.InvoiceNumber} اطمینان دارید؟");
        var dialogView = new ConfirmationDialogView { DataContext = dialogViewModel };

        var result = await DialogHost.Show(dialogView, "RootDialog");

        if (result is bool confirmation && confirmation)
        {
            try
            {
                var command = new DeleteInvoiceCommand { InvoiceId = invoice.Id, InvoiceType = InvoiceType };
                await _mediator.Send(command);
                _snackbarMessageQueue.Enqueue("فاکتور با موفقیت حذف شد.");
                await LoadInvoices();
            }
            catch (Exception ex)
            {
                _snackbarMessageQueue.Enqueue($"خطا در حذف فاکتور: {ex.Message}");
            }
        }
    }

    [RelayCommand]
    private void ManageInstallments(InvoiceListDto invoice)
    {
        _mainViewModel.NavigateToInstallmentManagement(invoice.Id, InvoiceType);
    }
    
    [RelayCommand]
    private async Task PrintInvoice(InvoiceListDto invoice)
    {
        if (invoice == null) return;

        try
        {
            var printViewModel = new PrintPreviewViewModel(_mediator, invoice.Id, InvoiceType);
            await printViewModel.InitializeAsync();

            var printWindow = new PrintPreviewWindow
            {
                DataContext = printViewModel
            };

            printWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            _snackbarMessageQueue.Enqueue($"خطا در آماده‌سازی فاکتور برای چاپ: {ex.Message}");
        }
    }
    
    partial void OnCurrentPageChanged(int value) => UpdatePagedInvoices();

    private bool CanGoToNextPage() => CurrentPage < TotalPages;
    [RelayCommand(CanExecute = nameof(CanGoToNextPage))]
    private void GoToNextPage() => CurrentPage++;

    private bool CanGoToPreviousPage() => CurrentPage > 1;
    [RelayCommand(CanExecute = nameof(CanGoToPreviousPage))]
    private void GoToPreviousPage() => CurrentPage--;
}