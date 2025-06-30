namespace StoreManagement.WPF.ViewModels.Invoicing;

public partial class AddPartyDialogViewModel : ViewModelBase
{
    private readonly IMediator _mediator;

    // یک پراپرتی برای مدیریت تب انتخاب شده (0 برای مشتری، 1 برای فروشگاه)
    [ObservableProperty]
    private int _selectedTabIndex = 0;

    // پراپرتی‌های مشترک
    [ObservableProperty] private string _phoneNumber;
    [ObservableProperty] private string _city;
    [ObservableProperty] private string _fullAddress;

    // پراپرتی‌های مشتری
    [ObservableProperty] private string _firstName;
    [ObservableProperty] private string _lastName;
    [ObservableProperty] private long? _nationalCode;

    // پراپرتی‌های فروشگاه
    [ObservableProperty] private string _storeName;

    public AddPartyDialogViewModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [RelayCommand]
    private async Task Save()
    {
        try
        {
            long newPartyId = 0;
            // بر اساس تب انتخاب شده، Command مناسب را ارسال می‌کنیم
            if (SelectedTabIndex == 0) // تب مشتری
            {
                var command = new CreateCustomerCommand
                {
                    FirstName = this.FirstName,
                    LastName = this.LastName,
                    NationalCode = this.NationalCode,
                    PhoneNumber = this.PhoneNumber,
                    City = this.City,
                    FullAddress = this.FullAddress
                };
                var result = await _mediator.Send(command);
                newPartyId = result.Value;
            }
            else // تب فروشگاه
            {
                var command = new CreateStoreCommand
                {
                    Name = this.StoreName,
                    Phone_Number = this.PhoneNumber,
                    Address_City = this.City,
                    Address_FullAddress = this.FullAddress,
                };
                var result = await _mediator.Send(command);
                newPartyId = result.Value;
            }

            DialogHost.Close("RootDialog", newPartyId);
        }
        catch (System.Exception ex)
        {
            // TODO: نمایش خطا به کاربر
            DialogHost.Close("RootDialog", null);
        }
    }
}