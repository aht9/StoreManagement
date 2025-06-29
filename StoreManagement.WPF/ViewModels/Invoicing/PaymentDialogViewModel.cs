
namespace StoreManagement.WPF.ViewModels.Invoicing;

public class PaymentResult
{
    public PaymentType PaymentType { get; set; }
    public long? BankAccountId { get; set; }
    public InstallmentDetailsDto InstallmentDetails { get; set; }
}

public partial class PaymentDialogViewModel : ViewModelBase
{
    private readonly IMediator _mediator;

    [ObservableProperty] private decimal _grandTotal;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCashPayment))]    
    [NotifyPropertyChangedFor(nameof(IsInstallmentPayment))]
    private PaymentType _selectedPaymentType = PaymentType.Cash;


    [ObservableProperty] private ObservableCollection<BankAccountDto> _bankAccounts;
    [ObservableProperty] private BankAccountDto _selectedBankAccount;

    [ObservableProperty] private decimal _downPayment;
    [ObservableProperty] private int _installmentMonths = 1;
    [ObservableProperty] private double _interestRate;

    public bool IsCashPayment => SelectedPaymentType == PaymentType.Cash;
    public bool IsInstallmentPayment => SelectedPaymentType == PaymentType.Installment;

    public PaymentDialogViewModel(IMediator mediator, decimal grandTotal)
    {
        _mediator = mediator;
        _grandTotal = grandTotal;
        Task.Run(LoadBankAccounts);
    }

    private async Task LoadBankAccounts()
    {
        var accounts = await _mediator.Send(new GetAllBankAccountsQuery());
        BankAccounts = new ObservableCollection<BankAccountDto>(accounts.Value);
    }

    public PaymentResult GetPaymentResult()
    {
        var result = new PaymentResult { PaymentType = this.SelectedPaymentType };
        if (SelectedPaymentType == PaymentType.Cash)
        {
            result.BankAccountId = SelectedBankAccount?.Id;
        }
        else if (SelectedPaymentType == PaymentType.Installment)
        {
            result.InstallmentDetails = new InstallmentDetailsDto
            {
                DownPayment = this.DownPayment,
                Months = this.InstallmentMonths,
                InterestRate = this.InterestRate
            };
            result.BankAccountId = SelectedBankAccount?.Id;
        }
        return result;
    }
}