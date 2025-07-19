namespace StoreManagement.WPF.ViewModels;

public partial class AddProductViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly Action _onSaveAction;
    private readonly Action _onCancelAction;

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string? _description;
    [ObservableProperty] private ProductCategoryTreeDto? _selectedCategory;
    [ObservableProperty] private ObservableCollection<ProductCategoryTreeDto> _availableCategories = new();
    [ObservableProperty] private bool _isBusy;

    public AddProductViewModel(IMediator mediator, Action onSaveAction, Action onCancelAction)
    {
        _mediator = mediator;
        _onSaveAction = onSaveAction;
        _onCancelAction = onCancelAction;
        LoadCategoriesAsync();
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save()
    {
        IsBusy = true;
        try
        {
            var command = new CreateProductCommand
            {
                Name = Name,
                Description = Description,
                CategoryId = SelectedCategory?.Id
            };

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                MessageBox.Show("محصول با موفقیت اضافه شد.", "موفقیت", MessageBoxButton.OK, MessageBoxImage.Information);
                _onSaveAction?.Invoke(); 
            }
            else
            {
                MessageBox.Show(result.Error, "خطا در افزودن محصول", MessageBoxButton.OK, MessageBoxImage.Error);
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

    private async void LoadCategoriesAsync()
    {
        IsBusy = true;
        try
        {
            var query = new GetAllProductCategoriesQuery();
            var result = await _mediator.Send(query);
            if (result.IsSuccess)
            {
                AvailableCategories = new ObservableCollection<ProductCategoryTreeDto>(result.Value);
            }
            else
            {
                MessageBox.Show(result.Error, "خطا در بارگذاری دسته‌بندی‌ها", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطای غیرمنتظره در بارگذاری دسته‌بندی‌ها: {ex.Message}", "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnNameChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
}