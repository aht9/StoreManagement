using MediatR;

namespace StoreManagement.WPF.ViewModels;


public partial class EditCustomerViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly Func<Task> _onSave;
    private readonly Action _onCancel;

    [ObservableProperty] private long _id;
    [ObservableProperty] private string _firstName = "";
    [ObservableProperty] private string _lastName = "";
    [ObservableProperty] private string? _email;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _phoneNumber = "";

    [ObservableProperty] private string _city = "";
    [ObservableProperty] private string _fullAddress = "";
    [ObservableProperty] private DateTime? _dateOfBirth;
    [ObservableProperty] private long? _nationalCode;
    [ObservableProperty] private bool _isBusy;

    public EditCustomerViewModel(IMediator mediator, CustomerDto customerToEdit, Func<Task> onSave, Action onCancel)
    {
        _mediator = mediator;
        _onSave = onSave;
        _onCancel = onCancel;

        // Load existing data into the form
        Id = customerToEdit.Id;
        FirstName = customerToEdit.FirstName;
        LastName = customerToEdit.LastName;
        PhoneNumber = customerToEdit.PhoneNumber;
        Email = customerToEdit.Email;
        City = customerToEdit.Address_City;
        FullAddress = customerToEdit.Address_FullAddress;
        DateOfBirth = customerToEdit.DateOfBirth;
        NationalCode = customerToEdit.NationalCode;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save()
    {
        IsBusy = true;
        try
        {
            var command = new UpdateCustomerCommand
            {
                Id = this.Id,
                PhoneNumber = this.PhoneNumber,
                Email = this.Email,
                City = this.City,
                FullAddress = this.FullAddress
            };
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                MessageBox.Show("Customer updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                await _onSave.Invoke();
            }
            else
            {
                MessageBox.Show(result.Error, "Update Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSave() => !string.IsNullOrWhiteSpace(PhoneNumber) && !IsBusy;

    [RelayCommand]
    private void Cancel()
    {
        _onCancel.Invoke();
    }
}
