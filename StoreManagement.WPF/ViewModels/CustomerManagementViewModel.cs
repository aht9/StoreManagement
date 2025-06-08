namespace StoreManagement.WPF.ViewModels;

public partial class CustomerManagementViewModel : ViewModelBase
{
    private List<Customer> _allCustomers;

    [ObservableProperty]
    private ObservableCollection<Customer> _pagedCustomers;

    [ObservableProperty]
    private AddCustomerViewModel _addCustomerViewModel;

    [ObservableProperty]
    private bool _isAddCustomerDialogOpen = false;

    [ObservableProperty]
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

    public int TotalPages => (_allCustomers == null || _allCustomers.Count == 0) ? 1 : (int)Math.Ceiling((double)FilteredCustomers.Count() / PageSize);


    [ObservableProperty]
    private EditCustomerViewModel _editCustomerViewModel;

    [ObservableProperty]
    private bool _isEditCustomerDialogOpen = false;


    private IEnumerable<Customer> FilteredCustomers =>
        string.IsNullOrWhiteSpace(SearchText)
            ? _allCustomers
            : _allCustomers.Where(c =>
                c.FirstName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                c.LastName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                (c.Email != null && c.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                c.Phone.Value.Contains(SearchText));

    public CustomerManagementViewModel()
    {
        LoadCustomers();
        UpdatePagedCustomers();
    }

    partial void OnSearchTextChanged(string value)
    {
        CurrentPage = 1; // Reset to first page on new search
        UpdatePagedCustomers();
    }

    partial void OnCurrentPageChanged(int value)
    {
        UpdatePagedCustomers();
    }

    private void UpdatePagedCustomers()
    {
        var customers = FilteredCustomers
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize);
        PagedCustomers = new ObservableCollection<Customer>(customers);
    }

    private void LoadCustomers()
    {
        // In a real application, this data would come from a repository/service.
        _allCustomers = new List<Customer>
        {
            new Customer("علیرضا","محمدی","mAli@Gmail.com","9120004568","تهران", "خیابان ولیعصر,پلاک 18",null,null),
            new Customer("Sara","Ahmadi","sara.ahmadi@example.com","9120004567","Shiraz", "Zand Street, No. 12",null,null),
            new Customer("Reza","Karimi","reza.karimi@example.com","9130001234","Isfahan", "Chaharbagh Street, No. 45",null,null),
            new Customer("Neda","Hosseini","neda.hosseini@example.com","9140005678","Mashhad", "Imam Reza Street, No. 78",null,null),
            new Customer("Ali","Jafari","ali.jafari@example.com","9150009876","Tabriz", "Shahnaz Street, No. 23",null,null),
            new Customer("Maryam","Rahimi","maryam.rahimi@example.com","9160003456","Qom", "Azar Street, No. 67",null,null),
            new Customer("Hossein","Ebrahimi","hossein.ebrahimi@example.com","9170007890","Kerman", "Shohada Street, No. 89",null,null),
            new Customer("Fatemeh","Shirazi","fatemeh.shirazi@example.com","9180001234","Ahvaz", "Kianpars Street, No. 12",null,null),
            new Customer("Mehdi","Ghasemi","mehdi.ghasemi@example.com","9190005678","Rasht", "Golsar Street, No. 34",null,null),
            new Customer("Zahra","Moradi","zahra.moradi@example.com","9200007890","Hamedan", "Baba Taher Street, No. 56",null,null),
            new Customer("Parsa","Nikzad","parsa.nikzad@example.com","9210001234","Yazd", "Amir Chakhmaq Street, No. 78",null,null),
            new Customer("Elham","Khalili","elham.khalili@example.com","9220005678","Kermanshah", "Azadi Street, No. 90",null,null),
        };
    }

    [RelayCommand]
    private void OpenAddCustomerDialog()
    {
        AddCustomerViewModel = new AddCustomerViewModel(
            // OnSave Action
            (newCustomer) => {
                _allCustomers.Add(newCustomer);
                UpdatePagedCustomers(); // Refresh the list
                IsAddCustomerDialogOpen = false;
            },
            // OnCancel Action
            () => {
                IsAddCustomerDialogOpen = false;
            }
        );
        IsAddCustomerDialogOpen = true;
    }

    [RelayCommand]
    private void EditCustomer(Customer customerToEdit)
    {
        if (customerToEdit == null) return;

        EditCustomerViewModel = new EditCustomerViewModel(customerToEdit,
            (updatedCustomer) =>
            {
                var index = _allCustomers.FindIndex(c => c.Email == customerToEdit.Email && c.Phone.Value == customerToEdit.Phone.Value); 
                if (index != -1)
                {
                    _allCustomers[index] = updatedCustomer;
                }
                UpdatePagedCustomers();
                IsEditCustomerDialogOpen = false;
            },

            () =>
            {
                IsEditCustomerDialogOpen = false;
            }
        );
        IsEditCustomerDialogOpen = true;
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
    private void DeleteCustomer(Customer customer)
    {
        // Add confirmation logic here
        _allCustomers.Remove(customer);
        UpdatePagedCustomers();
    }
}