namespace StoreManagement.WPF.ViewModels;

public partial class AddTransactionViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly long _bankAccountId;
    private readonly Func<Task> _onSave;
    private readonly Action _onCancel;

    [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveCommand))] private decimal _amount;
    [ObservableProperty] private TransactionType _transactionType = TransactionType.Credit;
    [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveCommand))] private string _description = "";
    [ObservableProperty] private bool _isBusy;

    public Array TransactionTypes => Enum.GetValues(typeof(TransactionType));

    public AddTransactionViewModel(IMediator mediator, long bankAccountId, Func<Task> onSave, Action onCancel)
    {
        _mediator = mediator;
        _bankAccountId = bankAccountId;
        _onSave = onSave;
        _onCancel = onCancel;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save()
    {
        IsBusy = true;
        var command = new CreateFinancialTransactionCommand
        {
            BankAccountId = _bankAccountId,
            Amount = Amount,
            TransactionType = TransactionType,
            Description = Description
        };
        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            MessageBox.Show("تراکنش با موفقیت ثبت شد.", "موفقیت", MessageBoxButton.OK, MessageBoxImage.Information);
            await _onSave();
        }
        else
        {
            MessageBox.Show(result.Error, "خطا در ثبت تراکنش", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        IsBusy = false;
    }

    private bool CanSave() => Amount > 0 && !string.IsNullOrWhiteSpace(Description) && !IsBusy;

    [RelayCommand]
    private void Cancel() => _onCancel();
}