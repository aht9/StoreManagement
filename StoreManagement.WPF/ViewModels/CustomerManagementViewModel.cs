using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StoreManagement.Domain.Aggregates.Customers;
using StoreManagement.Domain.ValueObjects;

namespace StoreManagement.WPF.ViewModels;

public partial class CustomerManagementViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<Customer> _customers;

    [ObservableProperty]
    private bool _isAddCustomerDialogOpen = false;

    [ObservableProperty]
    private AddCustomerViewModel _addCustomerViewModel;

    public CustomerManagementViewModel()
    {
        LoadCustomers();
    }

    private void LoadCustomers()
    {
        Customers = new ObservableCollection<Customer>
        {
            new Customer("علیرضا","محمدی","mAli@Gmail.com","09120004568","تهران", "خیابان ولیعصر,پلاک 18",null,null),
            new Customer("Sara","Ahmadi","sara.ahmadi@example.com","09120004567","Shiraz", "Zand Street, No. 12",null,null),
            new Customer("Reza","Karimi","reza.karimi@example.com","09130001234","Isfahan", "Chaharbagh Street, No. 45",null,null),
            new Customer("Neda","Hosseini","neda.hosseini@example.com","09140005678","Mashhad", "Imam Reza Street, No. 78",null,null),
            new Customer("Ali","Jafari","ali.jafari@example.com","09150009876","Tabriz", "Shahnaz Street, No. 23",null,null),
            new Customer("Maryam","Rahimi","maryam.rahimi@example.com","09160003456","Qom", "Azar Street, No. 67",null,null),
            new Customer("Hossein","Ebrahimi","hossein.ebrahimi@example.com","09170007890","Kerman", "Shohada Street, No. 89",null,null),
            new Customer("Fatemeh","Shirazi","fatemeh.shirazi@example.com","09180001234","Ahvaz", "Kianpars Street, No. 12",null,null),
            new Customer("Mehdi","Ghasemi","mehdi.ghasemi@example.com","09190005678","Rasht", "Golsar Street, No. 34",null,null),
            new Customer("Zahra","Moradi","zahra.moradi@example.com","09200007890","Hamedan", "Baba Taher Street, No. 56",null,null),
            new Customer("Parsa","Nikzad","parsa.nikzad@example.com","09210001234","Yazd", "Amir Chakhmaq Street, No. 78",null,null),
            new Customer("Elham","Khalili","elham.khalili@example.com","09220005678","Kermanshah", "Azadi Street, No. 90",null,null),
            new Customer("Sina","Rostami","sina.rostami@example.com","09230007890","Ardabil", "Shariati Street, No. 11",null,null),
            new Customer("Leila","Farhadi","leila.farhadi@example.com","09240001234","Bandar Abbas", "Imam Khomeini Street, No. 22",null,null),
            new Customer("Pouya","Shahidi","pouya.shahidi@example.com","09250005678","Zahedan", "Beheshti Street, No. 33",null,null),
            new Customer("Amin","Taheri","amin.taheri@example.com","09260007890","Sanandaj", "Enghelab Street, No. 44",null,null),
            new Customer("Shirin","Kazemi","shirin.kazemi@example.com","09270001234","Urmia", "Valiasr Street, No. 55",null,null),
            new Customer("Kian","Ramezani","kian.ramezani@example.com","09280005678","Qazvin", "Navvab Street, No. 66",null,null),
            new Customer("Hoda","Eskandari","hoda.eskandari@example.com","09290007890","Karaj", "Mehrshahr Street, No. 77",null,null)
        };
    }

    [RelayCommand]
    private void OpenAddCustomerDialog()
    {
        AddCustomerViewModel = new AddCustomerViewModel(
            // Success Action
            (newCustomer) => {
                Customers.Add(newCustomer);
                IsAddCustomerDialogOpen = false;
            },
            // Cancel Action
            () => {
                IsAddCustomerDialogOpen = false;
            }
        );
        IsAddCustomerDialogOpen = true;
    }
}