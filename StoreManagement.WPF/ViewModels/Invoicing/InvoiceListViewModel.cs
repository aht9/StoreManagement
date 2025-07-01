namespace StoreManagement.WPF.ViewModels.Invoicing;

public partial class InvoiceListViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly MainViewModel _mainViewModel;
    private readonly ISnackbarMessageQueue _snackbarMessageQueue;
    public InvoiceType InvoiceType { get; }

    [ObservableProperty]
    private ObservableCollection<InvoiceListDto> _invoices;

    // پراپرتی‌های مربوط به فیلتر
    [ObservableProperty] private DateTime? _startDate;
    [ObservableProperty] private DateTime? _endDate;
    [ObservableProperty] private string _selectedStatus = "همه"; // "همه", "پرداخت شده", "پرداخت نشده"

    public string PageTitle => InvoiceType == InvoiceType.Sales ? "مدیریت فاکتورهای فروش" : "مدیریت فاکتورهای خرید";

    public InvoiceListViewModel(IMediator mediator, MainViewModel mainViewModel, ISnackbarMessageQueue snackbarMessageQueue, InvoiceType invoiceType)
    {
        _mediator = mediator;
        _mainViewModel = mainViewModel;
        _snackbarMessageQueue = snackbarMessageQueue;
        InvoiceType = invoiceType;
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
                PaymentStatus = SelectedStatus == "همه" ? null : (SelectedStatus == "پرداخت شده" ? "Paid" : "Unpaid")
            };
            var result = await _mediator.Send(query);
            Invoices = new ObservableCollection<InvoiceListDto>(result);
        }
        catch (Exception ex)
        {
            _snackbarMessageQueue.Enqueue($"خطا در بارگذاری فاکتورها: {ex.Message}");
        }
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
}