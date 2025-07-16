namespace StoreManagement.WPF.ViewModels;

public partial class AddBankAccountViewModel(IMediator mediator, Func<Task> onSave, Action onCancel) : ViewModelBase
{
    [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveCommand))] private string _accountName = "";
    [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveCommand))] private string _bankName = "";
    [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveCommand))] private string _accountNumber = "";
    [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveCommand))] private string _cardNumberLastFour = "";
    [ObservableProperty] private decimal _initialBalance;
    [ObservableProperty] private bool _isBusy;

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save()
    {
        IsBusy = true;
        var command = new CreateBankAccountCommand
        {
            AccountName = AccountName,
            BankName = BankName,
            AccountNumber = AccountNumber,
            CardNumberLastFour = CardNumberLastFour,
            InitialBalance = InitialBalance
        };
        var result = await mediator.Send(command);
        if (result.IsSuccess)
        {
            MessageBox.Show("حساب بانکی با موفقیت ایجاد شد.", "موفقیت", MessageBoxButton.OK, MessageBoxImage.Information);
            await onSave();
        }
        else
        {
            MessageBox.Show(result.Error, "خطا در ایجاد", MessageBoxButton.OK, MessageBoxImage.Error);
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
    private void Cancel() => onCancel();
}