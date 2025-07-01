namespace StoreManagement.WPF.ViewModels.Invoicing;

public class PayInstallmentResult
{
    public decimal Amount { get; set; }
    public long BankAccountId { get; set; }
}

public partial class PayInstallmentDialogViewModel : ViewModelBase
{
    private readonly IMediator _mediator;

    [ObservableProperty]
    private decimal _amountToPay;

    [ObservableProperty]
    private ObservableCollection<BankAccountDto> _bankAccounts;

    [ObservableProperty]
    private BankAccountDto _selectedBankAccount;

    public PayInstallmentDialogViewModel(IMediator mediator)
    {
        _mediator = mediator;
        Task.Run(LoadBankAccounts);
    }

    private async Task LoadBankAccounts()
    {
        var accounts = await _mediator.Send(new GetAllBankAccountsQuery());
        BankAccounts = new ObservableCollection<BankAccountDto>(accounts.Value);
    }

    public PayInstallmentResult GetResult()
    {
        if (SelectedBankAccount == null || AmountToPay <= 0)
            return null;

        return new PayInstallmentResult
        {
            Amount = this.AmountToPay,
            BankAccountId = this.SelectedBankAccount.Id
        };
    }
}