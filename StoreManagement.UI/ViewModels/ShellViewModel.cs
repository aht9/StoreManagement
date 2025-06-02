namespace StoreManagement.UI.ViewModels;

public class ShellViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly ILogger _logger;
    private ViewModelBase? _currentViewModel;
    private NavigationItem? _selectedNavigationItem;
    private bool _isSidemenuOpen = true;

    public ViewModelBase? CurrentViewModel
    {
        get => _currentViewModel;
        private set => SetProperty(ref _currentViewModel, value);
    }

    public ObservableCollection<NavigationItem> NavigationItems { get; }

    public NavigationItem? SelectedNavigationItem
    {
        get => _selectedNavigationItem;
        set
        {
            if (SetProperty(ref _selectedNavigationItem, value) && value != null)
            {
                _logger.Information("Navigation item selected: {DisplayName}", value.DisplayName);
                _navigationService.NavigateTo(value.ViewModelType);
            }
        }
    }

    public bool IsSidemenuOpen
    {
        get => _isSidemenuOpen;
        set => SetProperty(ref _isSidemenuOpen, value);
    }

    public ICommand ToggleSidemenuCommand { get; }
    public ICommand LogoutCommand { get; }


    public ShellViewModel(INavigationService navigationService, ILogger logger)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _logger = logger?.ForContext<ShellViewModel>() ?? throw new ArgumentNullException(nameof(logger));

        _navigationService.CurrentViewModelChanged += OnCurrentViewModelChanged;
        Title = "مدیریت فروشگاه";

        NavigationItems = new ObservableCollection<NavigationItem>
        {
            new NavigationItem("Counter", typeof(CounterViewModel), PackIconKind.Counter),
            new NavigationItem("Dashboard", typeof(DashboardViewModel), PackIconKind.ViewDashboardOutline),
            new NavigationItem("Settings", typeof(SettingsViewModel), PackIconKind.CogOutline)
        };

        ToggleSidemenuCommand = new RelayCommand(() => IsSidemenuOpen = !IsSidemenuOpen);
        LogoutCommand = new RelayCommand(PerformLogout);


        _logger.Information("ShellViewModel initializing. Attempting to set initial page to CounterViewModel.");
        var initialItem = NavigationItems.FirstOrDefault(item => item.ViewModelType == typeof(CounterViewModel));
        if (initialItem != null)
        {
            SelectedNavigationItem = initialItem; 
        }
        else if (NavigationItems.Any())
        {
            _logger.Warning("CounterViewModel not found for initial navigation. Defaulting to first available item.");
            SelectedNavigationItem = NavigationItems.First(); 
        }
        else
        {
            _logger.Error("No navigation items configured in ShellViewModel.");
        }
    }

    private void OnCurrentViewModelChanged(ViewModelBase? newViewModel)
    {
        CurrentViewModel = newViewModel;
        if (newViewModel != null)
        {
            var navItem = NavigationItems.FirstOrDefault(item => item.ViewModelType == newViewModel.GetType());
            if (_selectedNavigationItem != navItem)
            {
                SetProperty(ref _selectedNavigationItem, navItem, nameof(SelectedNavigationItem));
            }
        }
    }

    private void PerformLogout()
    {
        _logger.Information("Logout action performed.");
        System.Windows.MessageBox.Show("Logout action triggered!", "Logout", System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Information);
    }
}