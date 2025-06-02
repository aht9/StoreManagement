namespace StoreManagement.UI.Services;

public class NavigationService: INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;
    private ViewModelBase? _currentViewModel;

    public ViewModelBase? CurrentViewModel
    {
        get => _currentViewModel;
        private set
        {
            _currentViewModel = value;
            _logger.Information("CurrentViewModel changed to {ViewModelType}", _currentViewModel?.GetType().Name ?? "null");
            CurrentViewModelChanged?.Invoke(_currentViewModel!);
        }
    }
    
    public event Action<ViewModelBase> CurrentViewModelChanged = delegate { };

    public NavigationService(IServiceProvider serviceProvider, ILogger logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger?.ForContext<NavigationService>() ?? throw new ArgumentNullException(nameof(logger));
    }

    public void NavigateTo(Type viewModelType)
    {
        if (viewModelType == null || !typeof(ViewModelBase).IsAssignableFrom(viewModelType))
        {
            _logger.Error("NavigateTo: Invalid viewModelType provided: {ViewModelTypeProvided}", viewModelType?.FullName);
            throw new ArgumentException("viewModelType must be a valid ViewModelBase type.", nameof(viewModelType));
        }

        _logger.Information("Navigating to ViewModel type: {ViewModelType}", viewModelType.FullName);

        try
        {
            var newViewModel = _serviceProvider.GetRequiredService(viewModelType) as ViewModelBase;
            CurrentViewModel = newViewModel;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error resolving or navigating to ViewModel type: {ViewModelType}", viewModelType.FullName);
            throw;
        }
    }
        
    public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
    {
        _logger.Information("Navigating to ViewModel type: {ViewModelType}", typeof(TViewModel).FullName);
        try
        {
            CurrentViewModel = _serviceProvider.GetRequiredService<TViewModel>();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error resolving or navigating to ViewModel type: {ViewModelType}", typeof(TViewModel).FullName);
            throw;
        }
    }
}