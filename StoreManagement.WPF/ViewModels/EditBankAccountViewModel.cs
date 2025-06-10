namespace StoreManagement.WPF.ViewModels;

public partial class EditBankAccountViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly Func<Task> _onSave;
    private readonly Action _onCancel;

    [ObservableProperty] private long _bankAccountId;
    [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveCommand))] private string _accountName;
    [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveCommand))] private string _bankName;
    [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveCommand))] private string _accountNumber;
    [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveCommand))] private string _cardNumberLastFour;
    [ObservableProperty] private bool _isBusy;

    public EditBankAccountViewModel(IMediator mediator, BankAccountDetailsDto account, Func<Task> onSave, Action onCancel)
    {
        _mediator = mediator;
        _onSave = onSave;
        _onCancel = onCancel;

        BankAccountId = account.Id;
        AccountName = account.AccountName;
        BankName = account.BankName;
        AccountNumber = account.AccountNumber;
        CardNumberLastFour = account.CardNumberLastFour;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save()
    {
        IsBusy = true;
        var command = new UpdateBankAccountCommand
        {
            Id = BankAccountId,
            AccountName = AccountName,
            BankName = BankName,
            AccountNumber = AccountNumber,
            CardNumberLastFour = CardNumberLastFour
        };
        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            MessageBox.Show("حساب بانکی با موفقیت ویرایش شد.", "موفقیت", MessageBoxButton.OK, MessageBoxImage.Information);
            await _onSave();
        }
        else
        {
            MessageBox.Show(result.Error, "خطا در ویرایش", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        IsBusy = false;
    }

    private bool CanSave() => !string.IsNullOrWhiteSpace(AccountName) &&
                              !string.IsNullOrWhiteSpace(BankName) &&
                              !string.IsNullOrWhiteSpace(AccountNumber) &&
                              !string.IsNullOrWhiteSpace(CardNumberLastFour) &&
                              CardNumberLastFour.Length == 4 &&
                              !IsBusy;

    [RelayCommand]
    private void Cancel() => _onCancel();
}