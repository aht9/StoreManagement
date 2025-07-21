using StoreManagement.WPF.Views;

namespace StoreManagement.WPF.ViewModels;

public partial class ProductCategoryManagementViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly ISnackbarMessageQueue _snackbarMessageQueue;


    [ObservableProperty] private ObservableCollection<ProductCategoryTreeDto> _categories;

    [ObservableProperty] private bool _isBusy;

    [ObservableProperty]
    private string _searchText = string.Empty;


    public ProductCategoryManagementViewModel(IMediator mediator, ISnackbarMessageQueue snackbarMessageQueue)
    {
        _mediator = mediator;
        _snackbarMessageQueue = snackbarMessageQueue;
        Task.Run(LoadCategoriesAsync);
    }

    [RelayCommand]
    private async Task LoadCategoriesAsync()
    {
        IsBusy = true;
        try
        {
            var query = new GetProductCategoryTreeQuery { SearchText = this.SearchText };
            var result = await _mediator.Send(query);
            if (result.IsSuccess)
            {
                Categories = new ObservableCollection<ProductCategoryTreeDto>(result.Value);
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

    partial void OnSearchTextChanged(string value)
    {
        LoadCategoriesCommand.Execute(null);
    }


    [RelayCommand]
    private async Task OpenAddDialogAsync(ProductCategoryTreeDto parentCategory)
    {
        var dialogViewModel = new AddProductCategoryViewModel(_mediator, _snackbarMessageQueue, parentCategory);
        var dialogView = new AddProductCategoryView { DataContext = dialogViewModel };
        var result = await DialogHost.Show(dialogView, "RootDialog");


        if (result is true)
        {
            await LoadCategoriesAsync(); 
        }
    }



    [RelayCommand]
    private async Task Refresh() => await LoadCategoriesAsync();
}