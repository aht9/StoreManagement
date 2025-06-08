namespace StoreManagement.WPF.ViewModels;

public partial class AddCustomerViewModel : ViewModelBase
{
    [ObservableProperty] private string _firstName;
    [ObservableProperty] private string _lastName;
    [ObservableProperty] private string _email;
    [ObservableProperty] private string _phone;
    [ObservableProperty] private string _city;
    [ObservableProperty] private string _fullAddress;
    [ObservableProperty] private string _dateOfBirth;
    [ObservableProperty] private string _nationalCode;

    private readonly Action<Customer> _onSave;
    private readonly Action _onCancel;

    public AddCustomerViewModel(Action<Customer> onSave, Action onCancel)
    {
        _onSave = onSave;
        _onCancel = onCancel;
    }

    [RelayCommand]
    private void Save()
    {
        var newCustomer = new Customer(
            FirstName,
            LastName,
            Email,
            Phone,
            City,
            FullAddress,
            DateTime.TryParse(DateOfBirth, out var dob) ? dob : (DateTime?)null,
            long.TryParse(NationalCode, out var nationalCode) ? nationalCode : (long?)null
        );
        _onSave?.Invoke(newCustomer);
    }

    [RelayCommand]
    private void Cancel()
    {
        _onCancel?.Invoke();
    }
}