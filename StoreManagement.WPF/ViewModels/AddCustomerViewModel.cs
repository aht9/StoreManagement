namespace StoreManagement.WPF.ViewModels;

public partial class AddCustomerViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _firstName = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _lastName = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _phoneNumber = "";

    [ObservableProperty] private string? _email;
    [ObservableProperty] private string _city;
    [ObservableProperty] private string _fullAddress;
    [ObservableProperty] private DateTime? _dateOfBirth;
    [ObservableProperty] private long? _nationalCode;
    [ObservableProperty] private bool _isBusy;

    private readonly IMediator _mediator;
    private readonly Func<Task> _onSave;
    private readonly Action _onCancel;

    public AddCustomerViewModel(IMediator mediator, Func<Task> onSave, Action onCancel)
    {
        _mediator = mediator;
        _onSave = onSave;
        _onCancel = onCancel;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save()
    {
        IsBusy = true;
        try
        {
            var command = new CreateCustomerCommand
            {
                FirstName = this.FirstName,
                LastName = this.LastName,
                PhoneNumber = this.PhoneNumber,
                Email = this.Email,
                City = this.City,
                FullAddress = this.FullAddress,
                DateOfBirth = this.DateOfBirth,
                NationalCode = this.NationalCode
            };
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                MessageBox.Show("Customer created successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                _onSave.Invoke();
            }
            else
            {
                MessageBox.Show(result.Error, "Creation Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(FirstName) &&
               !string.IsNullOrWhiteSpace(LastName) &&
               !string.IsNullOrWhiteSpace(PhoneNumber) &&
               !IsBusy;
    }

    [RelayCommand]
    private void Cancel()
    {
        _onCancel.Invoke();
    }
}