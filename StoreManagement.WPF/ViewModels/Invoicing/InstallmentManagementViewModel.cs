namespace StoreManagement.WPF.ViewModels.Invoicing;

public partial class InstallmentManagementViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly ISnackbarMessageQueue _snackbarMessageQueue;

    [ObservableProperty]
    private ObservableCollection<InstallmentDto> _installments;

    public long InvoiceId { get; }
    public InvoiceType InvoiceType { get; }
    public string PageTitle => $"مدیریت اقساط فاکتور شماره {InvoiceId}";

    public InstallmentManagementViewModel(IMediator mediator, ISnackbarMessageQueue snackbarMessageQueue, long invoiceId, InvoiceType invoiceType)
    {
        _mediator = mediator;
        _snackbarMessageQueue = snackbarMessageQueue;
        InvoiceId = invoiceId;
        InvoiceType = invoiceType;
        Task.Run(LoadInstallments);
    }

    [RelayCommand]
    private async Task LoadInstallments()
    {
        var query = new GetInstallmentsForInvoiceQuery { InvoiceId = this.InvoiceId, InvoiceType = this.InvoiceType };
        var result = await _mediator.Send(query);
        Installments = new ObservableCollection<InstallmentDto>(result);
    }

    [RelayCommand]
    private async Task PayInstallment(InstallmentDto installment)
    {
        var dialogViewModel = new PayInstallmentDialogViewModel(_mediator);
        var dialogView = new PayInstallmentDialogView { DataContext = dialogViewModel };

        var resultViewModel = await DialogHost.Show(dialogView, "RootDialog") as PayInstallmentDialogViewModel;
        if (resultViewModel == null) return; // کاربر دیالوگ را لغو کرده

        var paymentResult = resultViewModel.GetResult();
        if (paymentResult == null) return; 

        try
        {
            var command = new PayInstallmentCommand
            {
                InstallmentId = installment.Id,
                Amount = paymentResult.Amount,
                BankAccountId = paymentResult.BankAccountId,
                PaymentDate = DateTime.Now
            };
            await _mediator.Send(command);

            _snackbarMessageQueue.Enqueue("پرداخت با موفقیت ثبت شد.");

            // رفرش کردن لیست اقساط برای نمایش تغییرات
            await LoadInstallments();
        }
        catch (Exception ex)
        {
            _snackbarMessageQueue.Enqueue($"خطا در ثبت پرداخت: {ex.Message}");
        }
    }
}