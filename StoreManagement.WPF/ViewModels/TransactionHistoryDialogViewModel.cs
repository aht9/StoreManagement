namespace StoreManagement.WPF.ViewModels;

public partial class TransactionHistoryDialogViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private List<InventoryTransactionHistoryDto> _allHistoryItems = new();

    [ObservableProperty] private ObservableCollection<InventoryTransactionHistoryDto> _pagedHistory;

    public string ProductName { get; }
    public long ProductVariantId { get; }


    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GoToNextPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(GoToPreviousPageCommand))]
    private int _pageSize = 15;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GoToNextPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(GoToPreviousPageCommand))]
    private int _currentPage = 1;

    public int TotalPages => (_allHistoryItems == null || _allHistoryItems.Count == 0)
        ? 1
        : (int)Math.Ceiling((double)_allHistoryItems.Count / PageSize);


    public TransactionHistoryDialogViewModel(IMediator mediator, InventoryStatusDto selectedItem)
    {
        _mediator = mediator;
        ProductName = selectedItem.ProductName;
        ProductVariantId = selectedItem.ProductVariantId;
        Task.Run(() => LoadHistory(selectedItem.ProductVariantId));
    }

    private async Task LoadHistory(long productVariantId)
    {
        var query = new GetTransactionHistoryQuery { ProductVariantId = productVariantId };
        var result = await _mediator.Send(query);
        if (result.IsSuccess)
        {
            _allHistoryItems = result.Value;
            CurrentPage = 1; // Reset to first page
            OnPropertyChanged(nameof(TotalPages)); // Notify UI that total pages might have changed
            UpdatePagedHistory(); // Display the first page        }
        }
    }

    private void UpdatePagedHistory()
    {
        var historyPage = _allHistoryItems
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize);
        PagedHistory = new ObservableCollection<InventoryTransactionHistoryDto>(historyPage);
    }

    partial void OnCurrentPageChanged(int value) => UpdatePagedHistory();

    partial void OnPageSizeChanged(int value)
    {
        CurrentPage = 1;
        UpdatePagedHistory();
    }

    private bool CanGoToNextPage() => CurrentPage < TotalPages;

    [RelayCommand(CanExecute = nameof(CanGoToNextPage))]
    private void GoToNextPage() => CurrentPage++;

    private bool CanGoToPreviousPage() => CurrentPage > 1;

    [RelayCommand(CanExecute = nameof(CanGoToPreviousPage))]
    private void GoToPreviousPage() => CurrentPage--;
}