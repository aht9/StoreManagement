namespace StoreManagement.WPF.ViewModels;

public partial class AddStoreViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly Action _onSaveAction;
    private readonly Action _onCancelAction;

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string? _location;
    [ObservableProperty] private string? _managerName;
    [ObservableProperty] private string? _contactNumber;
    [ObservableProperty] private string? _email;
    [ObservableProperty] private string _phoneNumber = string.Empty; 
    [ObservableProperty] private string _addressCity = string.Empty; 
    [ObservableProperty] private string _addressFullAddress = string.Empty;
    [ObservableProperty] private bool _isBusy;

    public AddStoreViewModel(IMediator mediator, Action onSaveAction, Action onCancelAction)
    {
        _mediator = mediator;
        _onSaveAction = onSaveAction;
        _onCancelAction = onCancelAction;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save()
    {
        IsBusy = true;
        try
        {
            var command = new CreateStoreCommand
            {
                Name = Name,
                Location = Location,
                ManagerName = ManagerName,
                ContactNumber = ContactNumber,
                Email = Email,
                Phone_Number = PhoneNumber,
                Address_City = AddressCity,
                Address_FullAddress = AddressFullAddress
            };

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                MessageBox.Show("فروشگاه با موفقیت اضافه شد.", "موفقیت", MessageBoxButton.OK, MessageBoxImage.Information);
                _onSaveAction?.Invoke(); 
            }
            else
            {
                Log.Error("Failed to create store: {Error}", result.Error);
                MessageBox.Show(result.Error, "خطا در افزودن فروشگاه", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An unexpected error occurred while saving a new store.");
            MessageBox.Show($"خطای غیرمنتظره: {ex.Message}", "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(Name) &&
               !string.IsNullOrWhiteSpace(PhoneNumber) &&
               !string.IsNullOrWhiteSpace(AddressCity) &&
               !string.IsNullOrWhiteSpace(AddressFullAddress);
    }

    [RelayCommand]
    private void Cancel()
    {
        _onCancelAction?.Invoke(); 
    }

    partial void OnNameChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnPhoneNumberChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnAddressCityChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnAddressFullAddressChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
}