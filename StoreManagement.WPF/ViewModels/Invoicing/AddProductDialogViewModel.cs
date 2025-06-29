namespace StoreManagement.WPF.ViewModels.Invoicing;
public partial class AddProductDialogViewModel(IMediator mediator) : ViewModelBase
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddAndSelectProductCommand))]
    private string _productName;

    [ObservableProperty]
    private string _color;

    [ObservableProperty]
    private string _size;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddAndSelectProductCommand))]
    private string _sku;

    private bool CanAddAndSelectProduct()
    {
        return !string.IsNullOrWhiteSpace(ProductName) && !string.IsNullOrWhiteSpace(Sku);
    }

    [RelayCommand(CanExecute = nameof(CanAddAndSelectProduct))]
    private async Task AddAndSelectProduct()
    {
        var command = new QuickAddProductCommand
        {
            ProductName = this.ProductName,
            Color = this.Color,
            Size = this.Size,
            Sku = this.Sku
        };

        try
        {
            var resultDto = await mediator.Send(command);

            DialogHost.Close("RootDialog", resultDto);
        }
        catch (System.Exception ex)
        {
            // TODO: نمایش خطا به کاربر (مثلا با یک Snackbar)
            DialogHost.Close("RootDialog", null);
        }
    }
}