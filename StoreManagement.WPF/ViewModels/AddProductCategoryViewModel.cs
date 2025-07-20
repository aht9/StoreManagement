namespace StoreManagement.WPF.ViewModels;

public partial class AddProductCategoryViewModel(IMediator mediator, Action onSave, Action onCancel, ISnackbarMessageQueue snackbarMessageQueue) : ViewModelBase
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private int _order = 0;
    [ObservableProperty] private long? _parentCategoryId = 0;
    [ObservableProperty] private bool _isBusy;

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        _isBusy = true;
        try
        {
            var command = new CreateProductCategoryCommand
            {
                Name = Name,
                Description = Description,
                Order = Order,
                ParentCategoryId = ParentCategoryId
            };

            var result = await mediator.Send(command);
            if (result.IsSuccess)
            {
                snackbarMessageQueue.Enqueue("دسته بندی با موفقیت ایجاد شد.");
                onSave?.Invoke();
            }
            else
            {
                snackbarMessageQueue.Enqueue(result.Error);
            }
        }
        catch (Exception ex) 
        {
            snackbarMessageQueue.Enqueue($"خطای غیرمنتظره: {ex.Message}");
        }
        _isBusy = false;
    }

    private bool CanSave() => !string.IsNullOrWhiteSpace(Name) &&
                              !string.IsNullOrWhiteSpace(Description) &&
                              Order >= 0 &&
                              !_isBusy;


    [RelayCommand]
    private void Cancel() => onCancel?.Invoke();
}