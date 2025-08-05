namespace StoreManagement.WPF.ViewModels;

public partial class EditProductCategoryViewModel : ViewModelBase
{
    private readonly IMediator _mediator ;
    private readonly ISnackbarMessageQueue _snackbarMessageQueue ;
    private readonly long _categoryId;

    [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private int _order = 0;

    [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool _isBusy = false;
    
    [ObservableProperty]
    private ObservableCollection<ProductCategoryTreeDto> _availableCategories;

    [ObservableProperty]
    private ProductCategoryTreeDto _selectedParentCategory;

    public EditProductCategoryViewModel(IMediator mediator, 
        ISnackbarMessageQueue snackbarMessageQueue, long categoryId)
    {
        _mediator = mediator;
        _snackbarMessageQueue = snackbarMessageQueue;
        _categoryId = categoryId;
        
        Task.Run(LoadCategoryDetails);
    }

    private async Task LoadCategoryDetails()
    {
        IsBusy = true;
        try
        {
            var query = new GetProductCategoryDetailQuery { Id = _categoryId };
            var result = await _mediator.Send(query);
            if (result.IsSuccess)
            {
                Name = result.Value.Name;
                Description = result.Value.Description;
                Order = result.Value.Order;
                await LoadAvailableCategories(result.Value.ParentCategoryId);
            }
            else
            {
                _snackbarMessageQueue.Enqueue($"خطا در بارگذاری دسته بندی: {result.Error}");
            }
        }
        catch (Exception ex)
        {
            _snackbarMessageQueue.Enqueue($"خطا در بارگذاری دسته بندی: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    
    private async Task LoadAvailableCategories(long? parentCategoryId)
    {
        var query = new GetProductCategoryTreeQuery();
        var result = await _mediator.Send(query);
        if (result.IsSuccess)
        {
            var flatList = FlattenTree(result.Value);
            AvailableCategories = new ObservableCollection<ProductCategoryTreeDto>(flatList);
            if (parentCategoryId.HasValue)
            {
                SelectedParentCategory = AvailableCategories.FirstOrDefault(c => c.Id == parentCategoryId.Value);
            }
        }
    }
    
    private List<ProductCategoryTreeDto> FlattenTree(IEnumerable<ProductCategoryTreeDto> categories)
    {
        var list = new List<ProductCategoryTreeDto>();
        foreach (var category in categories)
        {
            list.Add(category);
            list.AddRange(FlattenTree(category.Subcategories));
        }
        return list;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        IsBusy = true;
        try
        {
            var command = new UpdateProductCategoryCommand
            {
                CategoryId = _categoryId,
                Name = Name,
                Description = Description,
                Order = Order,
                ParentCategoryId = SelectedParentCategory?.Id
            };
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
            {
                _snackbarMessageQueue.Enqueue("دسته بندی با موفقیت ویرایش شد.");
            }
            else
            {
                _snackbarMessageQueue.Enqueue($"خطا در ویرایش دسته بندی: {result.Error}");
            }
        }
        catch (Exception ex)
        {
            _snackbarMessageQueue.Enqueue($"خطا در ویرایش دسته بندی: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSave() => !IsBusy && !string.IsNullOrWhiteSpace(Name);
    
    [RelayCommand]
    private void Cancel()
    {
        DialogHost.Close("RootDialog", false); 
    }
}