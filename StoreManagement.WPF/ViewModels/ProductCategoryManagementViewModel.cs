namespace StoreManagement.WPF.ViewModels;

public partial class ProductCategoryManagementViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly ISnackbarMessageQueue _snackbarMessageQueue;


    private List<ProductCategoryTreeDto> _allProductCategories;

    [ObservableProperty] private ObservableCollection<ProductCategoryTreeDto> _pagedProductCategories = new();

    [ObservableProperty] private bool _isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedProductCategory))]
    private ProductCategoryTreeDto? _selectedProductCategory;

    //Dialog View Model
    [ObservableProperty] private AddProductCategoryViewModel _addProductCategoryViewModel;
    [ObservableProperty] private bool _isAddProductCategoryDialogOpen = false;


    //Pagination
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GoToNextPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(GoToPreviousPageCommand))]
    private int _pageSize = 10;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GoToNextPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(GoToPreviousPageCommand))] 
    private int _currentPage = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPages))]
    [NotifyCanExecuteChangedFor(nameof(GoToNextPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(GoToPreviousPageCommand))] 
    private string _searchText = string.Empty;

    private bool HasSelectedProductCategory => SelectedProductCategory != null;

    private IEnumerable<ProductCategoryTreeDto> FilteredCategoryProducts =>
        string.IsNullOrWhiteSpace(SearchText)
            ? _allProductCategories
            : _allProductCategories.Where(p =>
                p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                (p.Description != null && p.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));

    public int TotalPages => (_allProductCategories == null || _allProductCategories.Count == 0) ? 1 : (int)Math.Ceiling((double)FilteredCategoryProducts.Count() / PageSize);


    public ProductCategoryManagementViewModel(IMediator mediator, ISnackbarMessageQueue snackbarMessageQueue)
    {
        _mediator = mediator;
        _snackbarMessageQueue = snackbarMessageQueue;
    }

    [RelayCommand]
    private async Task LoadProductCategoriesAsync()
    {
        IsBusy = true;
        try
        {
            var query = new GetProductCategoryTreeQuery(){};
            var result = await _mediator.Send(query);
            if (result.IsSuccess)
            {
                _allProductCategories = result.Value;
                CurrentPage = 1;
                UpdatePagedProductsCategory();
            }
        }
        catch (Exception ex)
        {
            _snackbarMessageQueue.Enqueue($"خطا در بارگذاری دسته بندی ها: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void UpdatePagedProductsCategory()
    {
        var productCategory = _allProductCategories
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize);
        PagedProductCategories = new ObservableCollection<ProductCategoryTreeDto>(productCategory);
    }


    partial void OnSearchTextChanged(string value)
    {
        throw new NotImplementedException();
    }

    //Pagination Methods

    private bool CanGoToNextPage => CurrentPage < TotalPages;

    [RelayCommand]
    private void GoToNextPage()
    {
        CurrentPage++;
    }

    private bool CanGoToPreviousPage => CurrentPage > 1;

    [RelayCommand]
    private void GoToPreviousPage()
    {
        CurrentPage--;
    }
}