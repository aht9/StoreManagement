namespace StoreManagement.UI.ViewModels;

public class CounterViewModel : ViewModelBase
{
    private int _count;
    private readonly ILogger _logger;

    public int Count
    {
        get => _count;
        set
        {
            if (SetProperty(ref _count, value))
            {
                _logger.Information("Counter updated to {Count}", _count);
            }
        }
    }

    public ICommand IncrementCommand { get; }
    public ICommand DecrementCommand { get; }

    public CounterViewModel(ILogger logger)
    {
        _logger = logger?.ForContext<CounterViewModel>() ?? throw new ArgumentNullException(nameof(logger));
        Title = "Counter Page";
        Count = 0;
        IncrementCommand = new RelayCommand(Increment);
        DecrementCommand = new RelayCommand(Decrement, CanDecrement);

        _logger.Information("CounterViewModel initialized.");
    }

    private void Increment() => Count++;
    private void Decrement() => Count--;
    private bool CanDecrement() => Count > 0; // Example of CanExecute
}
