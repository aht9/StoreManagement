namespace StoreManagement.WPF.ViewModels;

public partial class ProductCategoryManagementViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private List<ProductCategoryDto> _allProductCategories;
    [ObservableProperty]
    private ObservableCollection<ProductCategoryDto> _pagedProductCategories;
    [ObservableProperty]

}