namespace StoreManagement.WPF.ViewModels;

public partial class AddProductVariantViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly Action _onSaveAction;
    private readonly Action _onCancelAction;
    private readonly long _productId;

    [ObservableProperty] private string _sku = string.Empty;
    [ObservableProperty] private string? _color;
    [ObservableProperty] private string? _size;
    [ObservableProperty] private bool _isBusy;

    public AddProductVariantViewModel(IMediator mediator, long productId, Action onSaveAction, Action onCancelAction)
    {
        _mediator = mediator;
        _productId = productId;
        _onSaveAction = onSaveAction;
        _onCancelAction = onCancelAction;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save()
    {
        IsBusy = true;
        try
        {
            var command = new CreateProductVariantCommand
            {
                ProductId = _productId,
                SKU = Sku,
                Color = Color,
                Size = Size
            };

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                MessageBox.Show("واریانت محصول با موفقیت اضافه شد.", "موفقیت", MessageBoxButton.OK, MessageBoxImage.Information);
                _onSaveAction?.Invoke();
            }
            else
            {
                MessageBox.Show(result.Error, "خطا در افزودن واریانت محصول", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطای غیرمنتظره: {ex.Message}", "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(Sku);
    }

    [RelayCommand]
    private void Cancel()
    {
        _onCancelAction?.Invoke(); 
    }

    partial void OnSkuChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
}