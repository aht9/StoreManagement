namespace StoreManagement.WPF.ViewModels;

public partial class StoreManagementViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private List<StoreDto> _allStores = new List<StoreDto>(); 

    [ObservableProperty] private ObservableCollection<StoreDto> _pagedStores = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedStore))]
    [NotifyCanExecuteChangedFor(nameof(OpenEditStoreDialogCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteStoreCommand))]
    private StoreDto? _selectedStore;

    [ObservableProperty] private bool _isBusy;

    // Dialog ViewModels
    [ObservableProperty] private AddStoreViewModel? _addStoreViewModel;
    [ObservableProperty] private bool _isAddStoreDialogOpen = false;

    [ObservableProperty] private EditStoreViewModel? _editStoreViewModel;
    [ObservableProperty] private bool _isEditStoreDialogOpen = false;

    public bool HasSelectedStore => SelectedStore != null;

    private IEnumerable<StoreDto> FilteredStores =>
        string.IsNullOrWhiteSpace(SearchText)
            ? _allStores
            : _allStores.Where(s =>
                s.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                (s.Location != null && s.Location.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                (s.ManagerName != null && s.ManagerName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                (s.ContactNumber != null && s.ContactNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                (s.Email != null && s.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                (s.Phone_Number != null && s.Phone_Number.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                (s.Address_City != null && s.Address_City.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                (s.Address_FullAddress != null && s.Address_FullAddress.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));

    public int TotalPages => (_allStores == null || _allStores.Count == 0) ? 1 : (int)Math.Ceiling((double)FilteredStores.Count() / PageSize);

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

    public StoreManagementViewModel(IMediator mediator)
    {
        _mediator = mediator;
        LoadStoresAsync();
    }

    [RelayCommand]
    private async Task LoadStoresAsync()
    {
        IsBusy = true;
        SelectedStore = null;
        try
        {
            var query = new GetAllStoresQuery { SearchText = this.SearchText };
            var result = await _mediator.Send(query);
            if (result.IsSuccess)
            {
                _allStores = result.Value;
                CurrentPage = 1;
                UpdatePagedStores();
            }
            else
            {
                Log.Error("Failed to load stores: {Error}", result.Error);
                MessageBox.Show(result.Error, "خطا در بارگذاری فروشگاه‌ها", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An unexpected error occurred while loading stores."); // Log unexpected exception
            MessageBox.Show($"خطای غیرمنتظره: {ex.Message}", "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void UpdatePagedStores()
    {
        var stores = FilteredStores
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize);
        PagedStores = new ObservableCollection<StoreDto>(stores);
    }

    partial void OnSearchTextChanged(string value)
    {
        CurrentPage = 1;
        LoadStoresAsync();
    }

    partial void OnCurrentPageChanged(int value)
    {
        UpdatePagedStores(); 
    }

    partial void OnPageSizeChanged(int value)
    {
        CurrentPage = 1;
        UpdatePagedStores();
    }

    [RelayCommand]
    private void OpenAddStoreDialog()
    {
        Action onSaveAction = async () =>
        {
            IsAddStoreDialogOpen = false;
            await LoadStoresAsync(); 
        };

        Action onCancelAction = () =>
        {
            IsAddStoreDialogOpen = false;
        };

        AddStoreViewModel = new AddStoreViewModel(_mediator, onSaveAction, onCancelAction);
        IsAddStoreDialogOpen = true;
    }

    [RelayCommand(CanExecute = nameof(HasSelectedStore))]
    private void OpenEditStoreDialog(StoreDto storeToEdit)
    {
        if (storeToEdit == null) return; 

        Action onSaveAction = async () =>
        {
            IsEditStoreDialogOpen = false;
            await LoadStoresAsync(); 
        };

        Action onCancelAction = () =>
        {
            IsEditStoreDialogOpen = false;
        };

        EditStoreViewModel = new EditStoreViewModel(_mediator, storeToEdit.Id, onSaveAction, onCancelAction);
        IsEditStoreDialogOpen = true;
    }

    [RelayCommand(CanExecute = nameof(HasSelectedStore))]
    private async Task DeleteStore(StoreDto storeToDelete)
    {
        if (storeToDelete == null) return; 

        var confirmResult = MessageBox.Show($"آیا مطمئن هستید که می‌خواهید فروشگاه {storeToDelete.Name} را حذف کنید؟",
            "تایید حذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (confirmResult == MessageBoxResult.No) return;

        IsBusy = true;
        try
        {
            var command = new DeleteStoreCommand { Id = storeToDelete.Id };
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
            {
                MessageBox.Show("فروشگاه با موفقیت حذف شد.", "موفقیت", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadStoresAsync();
            }
            else
            {
                Log.Error("Failed to delete store with Id {StoreId}: {Error}", storeToDelete.Id, result.Error);
                MessageBox.Show(result.Error, "حذف ناموفق", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An unexpected error occurred while deleting store with Id {StoreId}.", storeToDelete.Id);
            MessageBox.Show($"خطای غیرمنتظره: {ex.Message}", "خطای سیستمی", MessageBoxButton.OK, MessageBoxImage.Error);
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
    private async Task Refresh() => await LoadStoresAsync();
}