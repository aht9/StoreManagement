using StoreManagement.Application.DTOs.Products;
using StoreManagement.Application.Features.Products.Queries;

namespace StoreManagement.WPF.ViewModels;

public partial class ProductManagementViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private List<ProductDto> _allProducts;


    [ObservableProperty] private ObservableCollection<ProductDto> _pagedProducts = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedProduct))]
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
                c.CategoryName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
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
        AddProductViewModel = new AddProductViewModel(_mediator);
        IsAddProductDialogOpen = true;
    }

    [RelayCommand]
    private void OpenEditProductDialog()
    {
        if (SelectedProduct == null)
        {
            MessageBox.Show("Please select a product to edit.", "No Product Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        EditProductViewModel = new EditProductViewModel(_mediator, SelectedProduct.Id);
        IsEditProductDialogOpen = true;
    }


    [RelayCommand]
    private void OpenAddProductVariantDialog()
    {
        if (SelectedProduct == null)
        {
            MessageBox.Show("Please select a product to add a variant.", "No Product Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        AddProductVariantViewModel = new AddProductVariantViewModel(_mediator, SelectedProduct.Id);
        IsAddProductVariantDialogOpen = true;
    }

    [RelayCommand]
    private void OpenEditProductVariantDialog()
    {
        if (SelectedProduct == null)
        {
            MessageBox.Show("Please select a product variant to edit.", "No Variant Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        EditProductVariantViewModel = new EditProductVariantViewModel(_mediator, SelectedProduct.Id);
        IsEditProductVariantDialogOpen = true;
    }


    [RelayCommand]
    private void OpenEditCustomerDialog(ProductDto productToEdit)
    {
        if (productToEdit == null) return;

        EditProductViewModel = new EditProductViewModel(_mediator, productToEdit,
            // OnSave Action
            async () => {
                IsEditProductDialogOpen = false;
                await LoadProductsAsync(); // Refresh the list
            },
            // OnCancel Action
            () => {
                IsEditProductDialogOpen = false;
            }
        );
        IsEditProductDialogOpen = true;
    }

    [RelayCommand]
    private async Task DeleteCustomer(ProductDto productToDelete)
    {
        if (productToDelete == null) return;

        var confirmResult = MessageBox.Show($"Are you sure you want to delete {productToDelete.Name}?",
            "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (confirmResult == MessageBoxResult.No) return;

        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new DeleteCustomerCommand { Id = productToDelete.Id });
            if (result.IsSuccess)
            {
                MessageBox.Show("Customer deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadProductsAsync();
            }
            else
            {
                MessageBox.Show(result.Error, "Deletion Failed", MessageBoxButton.OK, MessageBoxImage.Error);
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