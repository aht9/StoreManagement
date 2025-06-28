namespace StoreManagement.WPF.ViewModels;

public partial class EditStoreViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly Action _onSaveAction;
    private readonly Action _onCancelAction;
    private readonly long _storeId;

    [ObservableProperty] private long _id;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string? _location;
    [ObservableProperty] private string? _managerName;
    [ObservableProperty] private string? _contactNumber;
    [ObservableProperty] private string? _email;
    [ObservableProperty] private string _phoneNumber = string.Empty; 
    [ObservableProperty] private string _addressCity = string.Empty; 
    [ObservableProperty] private string _addressFullAddress = string.Empty; 
    [ObservableProperty] private bool _isBusy;

    public EditStoreViewModel(IMediator mediator, long storeId, Action onSaveAction, Action onCancelAction)
    {
        _mediator = mediator;
        _storeId = storeId;
        _onSaveAction = onSaveAction;
        _onCancelAction = onCancelAction;
        LoadStoreDetailsAsync();
    }

    private async void LoadStoreDetailsAsync()
    {
        IsBusy = true;
        try
        {
            var query = new GetStoreByIdQuery { Id = _storeId };
            var result = await _mediator.Send(query);

            if (result.IsSuccess)
            {
                var store = result.Value;
                Id = store.Id;
                Name = store.Name;
                Location = store.Location;
                ManagerName = store.ManagerName;
                ContactNumber = store.ContactNumber;
                Email = store.Email;
                PhoneNumber = store.Phone_Number;
                AddressCity = store.Address_City;
                AddressFullAddress = store.Address_FullAddress;
            }
            else
            {
                Log.Error("Failed to load store details for Id {StoreId}: {Error}", _storeId, result.Error);
                MessageBox.Show(result.Error, "خطا در بارگذاری اطلاعات فروشگاه", MessageBoxButton.OK, MessageBoxImage.Error);
                _onCancelAction?.Invoke(); 
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An unexpected error occurred while loading store details for Id {StoreId}.", _storeId);
            MessageBox.Show($"خطای غیرمنتظره: {ex.Message}", "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
            _onCancelAction?.Invoke(); 
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save()
    {
        IsBusy = true;
        try
        {
            var command = new UpdateStoreCommand
            {
                Id = Id,
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
                MessageBox.Show("فروشگاه با موفقیت ویرایش شد.", "موفقیت", MessageBoxButton.OK, MessageBoxImage.Information);
                _onSaveAction?.Invoke(); 
            }
            else
            {
                Log.Error("Failed to update store with Id {StoreId}: {Error}", Id, result.Error);
                MessageBox.Show(result.Error, "خطا در ویرایش فروشگاه", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An unexpected error occurred while saving store updates for Id {StoreId}.", Id);
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