namespace StoreManagement.WPF.ViewModels;

public partial class ProductManagementViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private List<ProductDto> _allProducts;


    [ObservableProperty] private ObservableCollection<ProductDto> _pagedProducts = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedProduct))]
    [NotifyCanExecuteChangedFor(nameof(OpenEditProductDialogCommand))] // Added for general edit button
    [NotifyCanExecuteChangedFor(nameof(DeleteProductCommand))] // For general delete button
    [NotifyCanExecuteChangedFor(nameof(OpenAddProductVariantDialogCommand))] // To enable/disable variant buttons
    [NotifyCanExecuteChangedFor(nameof(OpenEditProductVariantDialogCommand))] // To enable/disable variant buttons
    private ProductDto? _selectedProduct;

    [ObservableProperty] private bool _isBusy;

    //Dialog ViewModels
    [ObservableProperty] private AddProductViewModel? _addProductViewModel;
    [ObservableProperty] private bool _isAddProductDialogOpen = false;

    [ObservableProperty] private EditProductViewModel? _editProductViewModel;
    [ObservableProperty] private bool _isEditProductDialogOpen = false;

    [ObservableProperty] private AddProductVariantViewModel? _addProductVariantViewModel;
    [ObservableProperty] private bool _isAddProductVariantDialogOpen = false;

    [ObservableProperty] private EditProductVariantViewModel? _editProductVariantViewModel;
    [ObservableProperty] private bool _isEditProductVariantDialogOpen = false;

    public bool HasSelectedProduct => SelectedProduct != null;

    private IEnumerable<ProductDto> FilteredProducts =>
        string.IsNullOrWhiteSpace(SearchText)
            ? _allProducts
            : _allProducts.Where(c =>
                c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                (c.CategoryName != null && c.CategoryName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) || // Added null check for CategoryName
                (c.Description != null && c.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));


    public int TotalPages => (_allProducts == null || _allProducts.Count == 0) ? 1 : (int)Math.Ceiling((double)FilteredProducts.Count() / PageSize);

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GoToNextPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(GoToPreviousPageCommand))]
    private int _pageSize = 10;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPages))]
    [NotifyCanExecuteChangedFor(nameof(GoToNextPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(GoToPreviousPageCommand))]
    private string _searchText = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GoToNextPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(GoToPreviousPageCommand))]
    private int _currentPage = 1;

    public ProductManagementViewModel(IMediator mediator)
    {
        _mediator = mediator;
        _allProducts = new List<ProductDto>();
        LoadProductsAsync();
    }

    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        IsBusy = true;
        SelectedProduct = null;
        try
        {
            var query = new GetAllProductsQuery { SearchText = this.SearchText };
            var result = await _mediator.Send(query);
            if (result.IsSuccess)
            {
                _allProducts = result.Value;
                CurrentPage = 1;
                UpdatePagedProducts();
            }
            else
            {
                MessageBox.Show(result.Error, "خطا در بارگذاری", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An unexpected error occurred: {ex.Message}", "System Error", MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }

    }

    private void UpdatePagedProducts()
    {
        var products = _allProducts
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize);
        PagedProducts = new ObservableCollection<ProductDto>(products);
    }

    partial void OnSearchTextChanged(string value)
    {
        CurrentPage = 1;
        LoadProductsAsync();
    }

    partial void OnCurrentPageChanged(int value)
    {
        LoadProductsAsync();
    }

    partial void OnPageSizeChanged(int value)
    {
        CurrentPage = 1;
        UpdatePagedProducts();
    }

    [RelayCommand]
    private void OpenAddProductDialog()
    {
        Action onSaveAction = async () =>
        {
            IsAddProductDialogOpen = false;
            await LoadProductsAsync(); // Refresh the list after adding
        };

        Action onCancelAction = () =>
        {
            IsAddProductDialogOpen = false;
        };

        AddProductViewModel = new AddProductViewModel(_mediator, onSaveAction, onCancelAction);
        IsAddProductDialogOpen = true;
    }

    [RelayCommand]
    private void OpenEditProductDialog(ProductDto productToEdit)
    {
        if (productToEdit == null) return;

        // OnSave Action for EditProductViewModel
        Action onSaveAction = async () =>
        {
            IsEditProductDialogOpen = false;
            await LoadProductsAsync(); // Refresh the list
        };

        // OnCancel Action for EditProductViewModel
        Action onCancelAction = () =>
        {
            IsEditProductDialogOpen = false;
        };

        EditProductViewModel = new EditProductViewModel(_mediator, productToEdit.Id, onSaveAction, onCancelAction);
        IsEditProductDialogOpen = true;
    }


    [RelayCommand(CanExecute = nameof(HasSelectedProduct))]
    private void OpenAddProductVariantDialog()
    {
        if (SelectedProduct == null)
        {
            MessageBox.Show("Please select a product to add a variant.", "No Product Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }


        Action onSaveAction = async () =>
        {
            IsAddProductVariantDialogOpen = false;
            await LoadProductsAsync();
        };

        Action onCancelAction = () =>
        {
            IsAddProductVariantDialogOpen = false;
        };

        AddProductVariantViewModel = new AddProductVariantViewModel(_mediator, SelectedProduct.Id, onSaveAction, onCancelAction);
        IsAddProductVariantDialogOpen = true;
    }

    [RelayCommand(CanExecute = nameof(HasSelectedProduct))]
    private void OpenEditProductVariantDialog()
    {
        if (SelectedProduct == null)
        {
            MessageBox.Show("Please select a product variant to edit.", "No Variant Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        Action onSaveAction = async () =>
        {
            IsEditProductVariantDialogOpen = false;
            await LoadProductsAsync(); 
        };

        Action onCancelAction = () =>
        {
            IsEditProductVariantDialogOpen = false;
        };

        EditProductVariantViewModel = new EditProductVariantViewModel(_mediator, SelectedProduct.Id, onSaveAction, onCancelAction);
        IsEditProductVariantDialogOpen = true;
    }

    [RelayCommand]
    private async Task DeleteProduct(ProductDto productToDelete) 
    {
        if (productToDelete == null) return;

        var confirmResult = MessageBox.Show($"آیا مطمئن هستید که می‌خواهید محصول {productToDelete.Name} را حذف کنید؟",
            "تایید حذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (confirmResult == MessageBoxResult.No) return;

        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new DeleteProductCommand { Id = productToDelete.Id }); // Changed to DeleteProductCommand
            if (result.IsSuccess)
            {
                MessageBox.Show("محصول با موفقیت حذف شد.", "موفقیت", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadProductsAsync();
            }
            else
            {
                MessageBox.Show(result.Error, "حذف ناموفق", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }


    [RelayCommand(CanExecute = nameof(CanGoToNextPage))]
    private void GoToNextPage()
    {
        CurrentPage++;
    }

    private bool CanGoToNextPage() => CurrentPage < TotalPages;

    [RelayCommand(CanExecute = nameof(CanGoToPreviousPage))]
    private void GoToPreviousPage()
    {
        CurrentPage--;
    }

    private bool CanGoToPreviousPage() => CurrentPage > 1;

    [RelayCommand]
    private async Task Refresh() => await LoadProductsAsync();
}