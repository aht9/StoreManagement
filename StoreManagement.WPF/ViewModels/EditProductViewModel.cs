namespace StoreManagement.WPF.ViewModels;

public partial class EditProductViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly Action _onSaveAction;
    private readonly Action _onCancelAction;
    private readonly long _productId;

    [ObservableProperty] private long _id;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string? _description;
    [ObservableProperty] private ProductCategoryTreeDto? _selectedCategory;
    [ObservableProperty] private ObservableCollection<ProductCategoryTreeDto> _availableCategories = new();
    [ObservableProperty] private bool _isBusy;

    public EditProductViewModel(IMediator mediator, long productId, Action onSaveAction, Action onCancelAction)
    {
        _mediator = mediator;
        _productId = productId;
        _onSaveAction = onSaveAction;
        _onCancelAction = onCancelAction;
        LoadProductDetailsAndCategoriesAsync();
    }

    private async void LoadProductDetailsAndCategoriesAsync()
    {
        IsBusy = true;
        try
        {
            var categoriesQuery = new GetAllProductCategoriesQuery();
            var categoriesResult = await _mediator.Send(categoriesQuery);
            if (categoriesResult.IsSuccess)
            {
                AvailableCategories = new ObservableCollection<ProductCategoryTreeDto>(categoriesResult.Value);
            }
            else
            {
                MessageBox.Show(categoriesResult.Error, "خطا در بارگذاری دسته‌بندی‌ها", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Then load Product Details
            var productQuery = new GetProductByIdQuery { Id = _productId };
            var productResult = await _mediator.Send(productQuery);

            if (productResult.IsSuccess)
            {
                var product = productResult.Value;
                Id = product.Id;
                Name = product.Name;
                Description = product.Description;
                SelectedCategory = AvailableCategories.FirstOrDefault(c => c.Id == product.CategoryId);
            }
            else
            {
                MessageBox.Show(productResult.Error, "خطا در بارگذاری اطلاعات محصول", MessageBoxButton.OK, MessageBoxImage.Error);
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

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save()
    {
        IsBusy = true;
        try
        {
            var command = new UpdateProductCommand
            {
                Id = Id,
                Name = Name,
                Description = Description,
                CategoryId = SelectedCategory?.Id
            };

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                MessageBox.Show("محصول با موفقیت ویرایش شد.", "موفقیت", MessageBoxButton.OK, MessageBoxImage.Information);
                _onSaveAction?.Invoke();
            }
            else
            {
                MessageBox.Show(result.Error, "خطا در ویرایش محصول", MessageBoxButton.OK, MessageBoxImage.Error);
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
        return !string.IsNullOrWhiteSpace(Name);
    }

    [RelayCommand]
    private void Cancel()
    {
        _onCancelAction?.Invoke(); 
    }

    partial void OnNameChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
}