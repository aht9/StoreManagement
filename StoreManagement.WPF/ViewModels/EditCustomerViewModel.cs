using StoreManagement.Domain.ValueObjects;

namespace StoreManagement.WPF.ViewModels;


public partial class EditCustomerViewModel : ViewModelBase
{
    private readonly Customer _customerToEdit;
    private readonly Action<Customer> _onSave;
    private readonly Action _onCancel;

    [ObservableProperty] private string _firstName;
    [ObservableProperty] private string _lastName;
    [ObservableProperty] private string _email;
    [ObservableProperty] private string _phone;
    [ObservableProperty] private string _city;
    [ObservableProperty] private string _fullAddress;
    [ObservableProperty] private string _dateOfBirth;
    [ObservableProperty] private string _nationalCode;

    public EditCustomerViewModel(Customer customer, Action<Customer> onSave, Action onCancel)
    {
        _customerToEdit = customer;
        _onSave = onSave;
        _onCancel = onCancel;

        // Load existing data into the form
        FirstName = customer.FirstName;
        LastName = customer.LastName;
        Email = customer.Email;
        Phone = customer.Phone.Value;
        City = customer.Address.City;
        FullAddress = customer.Address.FullAddress;
        DateOfBirth = customer.DateOfBirth?.ToString("yyyy/MM/dd");
        NationalCode = customer.NationalCode?.ToString();
    }

    [RelayCommand]
    private void Save()
    {
        // Update the original customer object
        // In a real-world scenario, you might use a method on the Customer entity
        // For now, we create a new instance with updated values, reflecting an update pattern
        var updatedCustomer = new Customer(
            FirstName,
            LastName,
            Email,
            Phone,
            City,
            FullAddress,
            DateTime.TryParse(DateOfBirth, out var dob) ? dob : (DateTime?)null,
            long.TryParse(NationalCode, out var nc) ? nc : (long?)null
        );
        // In a real app with Ids, you would assign the Id here: updatedCustomer.Id = _customerToEdit.Id;

        _onSave?.Invoke(updatedCustomer);
    }

    [RelayCommand]
    private void Cancel()
    {
        _onCancel?.Invoke();
    }
}
