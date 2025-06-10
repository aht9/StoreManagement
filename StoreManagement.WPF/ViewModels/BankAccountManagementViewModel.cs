namespace StoreManagement.WPF.ViewModels;

public partial class BankAccountManagementViewModel : ViewModelBase
{
    private readonly IMediator _mediator;

    [ObservableProperty]
    private ObservableCollection<BankAccountDto> _bankAccounts = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedAccount))]
    private BankAccountDetailsDto? _selectedAccountDetails;

    [ObservableProperty]
    private bool _isBusy;

    // Dialog ViewModels
    [ObservableProperty] private AddBankAccountViewModel? _addBankAccountViewModel;
    [ObservableProperty] private bool _isAddBankAccountDialogOpen;
    [ObservableProperty] private EditBankAccountViewModel? _editBankAccountViewModel;
    [ObservableProperty] private bool _isEditBankAccountDialogOpen;
    [ObservableProperty] private AddTransactionViewModel? _addTransactionViewModel;
    [ObservableProperty] private bool _isAddTransactionDialogOpen;

    public bool HasSelectedAccount => SelectedAccountDetails != null;

    public BankAccountManagementViewModel(IMediator mediator)
    {
        _mediator = mediator;
        LoadBankAccountsAsync();
    }

    partial void OnSelectedAccountDetailsChanged(BankAccountDetailsDto? value)
    {
        OnPropertyChanged(nameof(HasSelectedAccount));

        AddTransactionCommand.NotifyCanExecuteChanged();
        DeleteAccountCommand.NotifyCanExecuteChanged();
        EditAccountCommand.NotifyCanExecuteChanged();
        DeleteTransactionCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private async Task LoadBankAccountsAsync()
    {
        IsBusy = true;
        SelectedAccountDetails = null;
        var result = await _mediator.Send(new GetAllBankAccountsQuery());
        if (result.IsSuccess)
        {
            BankAccounts = new ObservableCollection<BankAccountDto>(result.Value);
        }
        else
        {
            MessageBox.Show(result.Error, "خطا در بارگذاری", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        IsBusy = false;
    }

    [RelayCommand]
    private async Task SelectBankAccount(BankAccountDto? selectedAccount)
    {
        if (selectedAccount == null)
        {
            SelectedAccountDetails = null;
            return;
        }
        IsBusy = true;
        var result = await _mediator.Send(new GetBankAccountDetailsQuery { BankAccountId = selectedAccount.Id });
        if (result.IsSuccess)
        {
            SelectedAccountDetails = result.Value;
        }
        else
        {
            MessageBox.Show(result.Error, "خطا در دریافت جزئیات", MessageBoxButton.OK, MessageBoxImage.Error);
            SelectedAccountDetails = null;
        }
        IsBusy = false;
    }

    [RelayCommand]
    private void OpenAddAccountDialog()
    {
        AddBankAccountViewModel = new AddBankAccountViewModel(_mediator, async () => {
            IsAddBankAccountDialogOpen = false;
            await LoadBankAccountsAsync();
        }, () => IsAddBankAccountDialogOpen = false);
        IsAddBankAccountDialogOpen = true;
    }

    [RelayCommand(CanExecute = nameof(CanManipulateAccount))]
    private void EditAccount()
    {
        if (SelectedAccountDetails == null) return;
        EditBankAccountViewModel = new EditBankAccountViewModel(_mediator, SelectedAccountDetails, async () => {
            IsEditBankAccountDialogOpen = false;
            await LoadBankAccountsAsync();
        }, () => IsEditBankAccountDialogOpen = false);
        IsEditBankAccountDialogOpen = true;
    }

    [RelayCommand(CanExecute = nameof(CanManipulateAccount))]
    private void AddTransaction()
    {
        if (SelectedAccountDetails == null) return;
        AddTransactionViewModel = new AddTransactionViewModel(_mediator, SelectedAccountDetails.Id, async () => {
            IsAddTransactionDialogOpen = false;
            await SelectBankAccount(SelectedAccountDetails); // Refresh details
            await LoadBankAccountsAsync(); // Refresh summary list
        }, () => IsAddTransactionDialogOpen = false);
        IsAddTransactionDialogOpen = true;
    }

    [RelayCommand(CanExecute = nameof(CanManipulateAccount))]
    private async Task DeleteAccount()
    {
        if (SelectedAccountDetails == null) return;
        var result = MessageBox.Show($"آیا از حذف حساب '{SelectedAccountDetails.AccountName}' اطمینان دارید؟", "تایید حذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result == MessageBoxResult.No) return;

        IsBusy = true;
        var commandResult = await _mediator.Send(new DeleteBankAccountCommand { Id = SelectedAccountDetails.Id });
        if (commandResult.IsSuccess)
        {
            MessageBox.Show("حساب با موفقیت حذف شد.", "موفقیت", MessageBoxButton.OK, MessageBoxImage.Information);
            await LoadBankAccountsAsync();
        }
        else
        {
            MessageBox.Show(commandResult.Error, "خطا در حذف", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        IsBusy = false;
    }

    private bool CanManipulateAccount() => SelectedAccountDetails != null;


    [RelayCommand(CanExecute = nameof(CanManipulateTransaction))]
    private async Task DeleteTransaction(FinancialTransactionDto? transaction)
    {
        if (SelectedAccountDetails == null || transaction == null) return;

        var result = MessageBox.Show($"آیا از حذف تراکنش '{transaction.Description}' به مبلغ {transaction.Amount} اطمینان دارید؟", "تایید حذف تراکنش", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result == MessageBoxResult.No) return;

        IsBusy = true;
        var command = new DeleteFinancialTransactionCommand
        {
            BankAccountId = SelectedAccountDetails.Id,
            TransactionId = transaction.Id
        };

        var commandResult = await _mediator.Send(command);
        if (commandResult.IsSuccess)
        {
            MessageBox.Show("تراکنش با موفقیت حذف شد.", "موفقیت", MessageBoxButton.OK, MessageBoxImage.Information);
            // Refresh data
            await SelectBankAccount(SelectedAccountDetails);
            await LoadBankAccountsAsync();
        }
        else
        {
            MessageBox.Show(commandResult.Error, "خطا در حذف", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        IsBusy = false;
    }

    private bool CanManipulateTransaction(FinancialTransactionDto? transaction) => transaction != null && SelectedAccountDetails != null;
}