namespace StoreManagement.WPF.ViewModels;

public partial class CustomerManagementViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private List<CustomerDto> _allCustomers;

    [ObservableProperty]
    private ObservableCollection<CustomerDto> _pagedCustomers;

    [ObservableProperty]
    private AddCustomerViewModel _addCustomerViewModel;

    [ObservableProperty]
    private bool _isAddCustomerDialogOpen = false;

    [ObservableProperty]
    private EditCustomerViewModel _editCustomerViewModel;

    [ObservableProperty]
    private bool _isEditCustomerDialogOpen = false;

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

    [ObservableProperty]
    private bool _isBusy;

    public int TotalPages => (_allCustomers == null || _allCustomers.Count == 0) ? 1 : (int)Math.Ceiling((double)FilteredCustomers.Count() / PageSize);


    private IEnumerable<CustomerDto> FilteredCustomers =>
        string.IsNullOrWhiteSpace(SearchText)
            ? _allCustomers
            : _allCustomers.Where(c =>
                c.FirstName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                c.LastName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                (c.Email != null && c.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                c.PhoneNumber.Contains(SearchText));

    public CustomerManagementViewModel(IMediator mediator)
    {
        _mediator = mediator;
        LoadCustomersAsync();
    }

    partial void OnSearchTextChanged(string value)
    {
        CurrentPage = 1; 
        LoadCustomersAsync();
    }

    partial void OnCurrentPageChanged(int value)
    {
        UpdatePagedCustomers();
    }

    partial void OnPageSizeChanged(int value)
    {
        CurrentPage = 1; 
        UpdatePagedCustomers();
    }

    private void UpdatePagedCustomers()
    {
        var customers = _allCustomers
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize);
        PagedCustomers = new ObservableCollection<CustomerDto>(customers);
    }

    private async Task LoadCustomersAsync()
    {
        IsBusy = true;
        try
        {
            var query = new GetAllCustomersQuery { SearchText = this.SearchText };
            var result = await _mediator.Send(query);
            if (result.IsSuccess)
            {
                _allCustomers = result.Value;
                CurrentPage = 1; 
                UpdatePagedCustomers();
            }
            else
            {
                MessageBox.Show(result.Error, "Error Loading Customers", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An unexpected error occurred: {ex.Message}", "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void OpenAddCustomerDialog()
    {
        AddCustomerViewModel = new AddCustomerViewModel(_mediator,
            // OnSave Action
            async () => {
                IsAddCustomerDialogOpen = false;
                await LoadCustomersAsync(); // Refresh the list from the database
            },
            // OnCancel Action
            () => {
                IsAddCustomerDialogOpen = false;
            }
        );
        IsAddCustomerDialogOpen = true;
    }

    [RelayCommand]
    private void OpenEditCustomerDialog(CustomerDto customerToEdit)
    {
        if (customerToEdit == null) return;

        EditCustomerViewModel = new EditCustomerViewModel(_mediator, customerToEdit,
            // OnSave Action
            async () => {
                IsEditCustomerDialogOpen = false;
                await LoadCustomersAsync(); // Refresh the list
            },
            // OnCancel Action
            () => {
                IsEditCustomerDialogOpen = false;
            }
        );
        IsEditCustomerDialogOpen = true;
    }

    [RelayCommand]
    private async Task DeleteCustomer(CustomerDto customerToDelete)
    {
        if (customerToDelete == null) return;

        var confirmResult = MessageBox.Show($"Are you sure you want to delete {customerToDelete.FullName}?",
            "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (confirmResult == MessageBoxResult.No) return;

        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new DeleteCustomerCommand { Id = customerToDelete.Id });
            if (result.IsSuccess)
            {
                MessageBox.Show("Customer deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadCustomersAsync();
            }
            else
            {
                MessageBox.Show(result.Error, "Deletion Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
    private async Task Refresh() => await LoadCustomersAsync();

}