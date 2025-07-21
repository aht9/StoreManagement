namespace StoreManagement.WPF.ViewModels;

public partial class TransactionHistoryDialogViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
        
    [ObservableProperty]
    private ObservableCollection<InventoryTransactionHistoryDto> _history;
        
    public string ProductName { get; set; }

    public TransactionHistoryDialogViewModel(IMediator mediator, InventoryStatusDto selectedItem)
    {
        _mediator = mediator;
        ProductName = selectedItem.ProductName;
        Task.Run(() => LoadHistory(selectedItem.ProductVariantId));
    }

    private async Task LoadHistory(long productVariantId)
    {
        var query = new GetTransactionHistoryQuery { ProductVariantId = productVariantId };
        var result = await _mediator.Send(query);
        if (result.IsSuccess)
        {
            History = new ObservableCollection<InventoryTransactionHistoryDto>(result.Value);
        }
    }
}