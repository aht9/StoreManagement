namespace StoreManagement.WPF.ViewModels;

public partial class InventoryManagementViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly ISnackbarMessageQueue _snackbarMessageQueue;

    [ObservableProperty]
    private ObservableCollection<InventoryStatusDto> _inventoryItems;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _searchText = string.Empty;

    public InventoryManagementViewModel(IMediator mediator, ISnackbarMessageQueue snackbarMessageQueue)
    {
        _mediator = mediator;
        _snackbarMessageQueue = snackbarMessageQueue;
        Task.Run(LoadInventoryAsync);
    }
    
    [RelayCommand]
    private async Task LoadInventoryAsync()
    {
        IsBusy = true;
        try
        {
            var query = new GetInventoryStatusQuery { SearchText = SearchText };
            var result = await _mediator.Send(query);
            if (result.IsSuccess)
            {
                InventoryItems = new ObservableCollection<InventoryStatusDto>(result.Value);
            }
            else
            {
                _snackbarMessageQueue.Enqueue($"Error: {result.Error}");
            }
        }
        catch (Exception ex)
        {
            _snackbarMessageQueue.Enqueue($"An unexpected error occurred: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    partial void OnSearchTextChanged(string value)
    {
        Task.Run(LoadInventoryAsync);
        
    }
    
    
    [RelayCommand]
    private async Task ShowHistory(InventoryStatusDto item)
    {
        if (item == null) return;

        var dialogViewModel = new TransactionHistoryDialogViewModel(_mediator, item);
        var dialogView = new TransactionHistoryDialogView { DataContext = dialogViewModel };
        
        await DialogHost.Show(dialogView, "RootDialog");
    }
}