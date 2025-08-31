namespace StoreManagement.WPF.ViewModels;

public partial class EditProductVariantViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly Action _onSaveAction;
    private readonly Action _onCancelAction;
    private readonly long _productId;

    [ObservableProperty] private ObservableCollection<ProductVariantDto> _variants = new();
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedVariant))]
    [NotifyCanExecuteChangedFor(nameof(SaveUpdateVariantCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteVariantCommand))]
    private ProductVariantDto? _selectedVariant;

    [ObservableProperty] private string _currentSku = string.Empty;
    [ObservableProperty] private string? _currentColor;
    [ObservableProperty] private string? _currentSize;
    [ObservableProperty] private bool _isBusy;

    public bool HasSelectedVariant => SelectedVariant != null;

    public EditProductVariantViewModel(IMediator mediator, long productId, Action onSaveAction, Action onCancelAction)
    {
        _mediator = mediator;
        _productId = productId;
        _onSaveAction = onSaveAction;
        _onCancelAction = onCancelAction;
        LoadVariantsAsync();
    }

    [RelayCommand]
    private async Task LoadVariantsAsync()
    {
        IsBusy = true;
        SelectedVariant = null;
        ClearCurrentVariantFields();
        try
        {
            var query = new GetVariantsForProductQuery { ProductId = _productId };
            var result = await _mediator.Send(query);
            if (result.IsSuccess)
            {
                Variants = new ObservableCollection<ProductVariantDto>(result.Value);
            }
            else
            {
                MessageBox.Show(result.Error, "خطا در بارگذاری واریانت‌ها", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطای غیرمنتظره در بارگذاری واریانت‌ها: {ex.Message}", "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanSaveUpdateVariant))]
    private async Task SaveUpdateVariant()
    {
        IsBusy = true;
        try
        {
            if (SelectedVariant != null && SelectedVariant.Id != 0)
            {
                var command = new UpdateProductVariantCommand
                {
                    Id = SelectedVariant.Id,
                    SKU = CurrentSku,
                    Color = CurrentColor,
                    Size = CurrentSize,
                };
                var result = await _mediator.Send(command);
                if (result.IsSuccess)
                {
                    MessageBox.Show("واریانت با موفقیت ویرایش شد.", "موفقیت", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadVariantsAsync();
                }
                else
                {
                    MessageBox.Show(result.Error, "خطا در ویرایش واریانت", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else // New variant, create
            {
                var command = new CreateProductVariantCommand
                {
                    ProductId = _productId,
                    SKU = CurrentSku,
                    Color = CurrentColor,
                    Size = CurrentSize
                };
                var result = await _mediator.Send(command);
                if (result.IsSuccess)
                {
                    MessageBox.Show("واریانت جدید با موفقیت اضافه شد.", "موفقیت", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadVariantsAsync();
                }
                else
                {
                    MessageBox.Show(result.Error, "خطا در افزودن واریانت جدید", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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

    private bool CanSaveUpdateVariant()
    {
        return !string.IsNullOrWhiteSpace(CurrentSku);
    }

    [RelayCommand(CanExecute = nameof(HasSelectedVariant))]
    private async Task DeleteVariant()
    {
        if (SelectedVariant == null) return;

        var confirmResult = MessageBox.Show($"آیا مطمئن هستید که می‌خواهید واریانت با SKU '{SelectedVariant.SKU}' را حذف کنید؟",
            "تایید حذف واریانت", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (confirmResult == MessageBoxResult.No) return;

        IsBusy = true;
        try
        {
            var command = new DeleteProductVariantCommand { Id = SelectedVariant.Id };
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                MessageBox.Show("واریانت با موفقیت حذف شد.", "موفقیت", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadVariantsAsync();
            }
            else
            {
                MessageBox.Show(result.Error, "خطا در حذف واریانت", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NewVariant()
    {
        SelectedVariant = null;
        IsBusy = true;
        try
        {
            var command = new CreateProductVariantCommand
            {
                ProductId = _productId,
                SKU = CurrentSku,
                Color = CurrentColor,
                Size = CurrentSize
            };

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                await LoadVariantsAsync();
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
        ClearCurrentVariantFields();
    }

    private void ClearCurrentVariantFields()
    {
        CurrentSku = string.Empty;
        CurrentColor = null;
        CurrentSize = null;
    }

    partial void OnSelectedVariantChanged(ProductVariantDto? value)
    {
        if (value != null)
        {
            CurrentSku = value.SKU;
            CurrentColor = value.Color;
            CurrentSize = value.Size;
        }
        else
        {
            ClearCurrentVariantFields();
        }
        SaveUpdateVariantCommand.NotifyCanExecuteChanged();
        DeleteVariantCommand.NotifyCanExecuteChanged();
    }

    partial void OnCurrentSkuChanged(string value) => SaveUpdateVariantCommand.NotifyCanExecuteChanged();

    [RelayCommand]
    private void Cancel()
    {
        _onCancelAction?.Invoke();
    }
}