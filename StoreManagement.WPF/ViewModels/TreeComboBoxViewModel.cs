namespace StoreManagement.WPF.ViewModels;

public partial class TreeComboBoxViewModel : ViewModelBase
{
    private readonly IMediator _mediator;

    [ObservableProperty]
    private ObservableCollection<ProductCategoryTreeDto> _categories;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedCategoryName))]
    private ProductCategoryTreeDto _selectedCategory;

    [ObservableProperty]
    private bool _isDropDownOpen;

    [ObservableProperty]
    private string _searchText;

    public string SelectedCategoryName => SelectedCategory?.Name ?? "انتخاب دسته بندی...";


    public TreeComboBoxViewModel(IMediator mediator)
    {
        _mediator = mediator;
        Task.Run(LoadCategoriesAsync);
    }

    partial void OnSearchTextChanged(string value) => Task.Run(LoadCategoriesAsync);

    private async Task LoadCategoriesAsync()
    {
        var result = await _mediator.Send(new GetProductCategoryTreeQuery { SearchText = SearchText });
        if (result.IsSuccess)
        {
            Categories = new ObservableCollection<ProductCategoryTreeDto>(result.Value);
        }
    }

    public void SelectCategory(ProductCategoryTreeDto category)
    {
        SelectedCategory = category;
        IsDropDownOpen = false; 
    }
}