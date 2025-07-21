namespace StoreManagement.WPF.ViewModels;

public partial class AddProductCategoryViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly ISnackbarMessageQueue _snackbarMessageQueue;
    private readonly long? _preselectedParentId;

    [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveCommand))] private string _name = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private int _order = 0;
    [ObservableProperty][NotifyCanExecuteChangedFor(nameof(SaveCommand))] private bool _isBusy = false;

    [ObservableProperty]
    private ObservableCollection<ProductCategoryTreeDto> _availableCategories;

    [ObservableProperty]
    private ProductCategoryTreeDto _selectedParentCategory;

    public AddProductCategoryViewModel(IMediator mediator, ISnackbarMessageQueue snackbarMessageQueue, ProductCategoryTreeDto? categoryTreeDto )
    {
        _mediator = mediator;
        _snackbarMessageQueue = snackbarMessageQueue;
        _preselectedParentId = categoryTreeDto?.Id; // Store the incoming parent's ID
        Task.Run(LoadAvailableCategories);
    }

    private async Task LoadAvailableCategories()
    {
        var query = new GetProductCategoryTreeQuery();
        var result = await _mediator.Send(query);
        if (result.IsSuccess)
        {
            var flatList = FlattenTree(result.Value);
            AvailableCategories = new ObservableCollection<ProductCategoryTreeDto>(flatList);
            if (_preselectedParentId.HasValue)
            {
                SelectedParentCategory = AvailableCategories.FirstOrDefault(c => c.Id == _preselectedParentId.Value);
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
            var command = new CreateProductCategoryCommand
            {
                Name = this.Name,
                Description = this.Description,
                Order = this.Order,
                ParentCategoryId = SelectedParentCategory?.Id
            };

            var result = await _mediator.Send(command);
            if (result.IsSuccess)
            {
                _snackbarMessageQueue.Enqueue("دسته‌بندی با موفقیت ایجاد شد.");
                DialogHost.Close("RootDialog", true);
            }
            else
            {
                _snackbarMessageQueue.Enqueue(result.Error);
            }
        }
        catch (Exception ex)
        {
            _snackbarMessageQueue.Enqueue($"خطای غیرمنتظره: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }


    private bool CanSave() => !string.IsNullOrWhiteSpace(Name) && !IsBusy;

    [RelayCommand]
    private void Cancel()
    {
        DialogHost.Close("RootDialog", false); 
    }
}