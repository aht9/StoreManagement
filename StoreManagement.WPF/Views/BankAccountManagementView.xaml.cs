using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StoreManagement.WPF.Views
{
    /// <summary>
    /// Interaction logic for BankAccountManagementView.xaml
    /// </summary>
    public partial class BankAccountManagementView : UserControl
    {
        public BankAccountManagementView()
        {
            InitializeComponent();
        }

        private void ListBoxItem_Selected(object sender, RoutedEventArgs e)
        {
            if (DataContext is BankAccountManagementViewModel viewModel &&
                sender is ListBoxItem { DataContext: BankAccountDto selectedAccount })
            {
                if (viewModel.SelectBankAccountCommand.CanExecute(selectedAccount))
                {
                    viewModel.SelectBankAccountCommand.Execute(selectedAccount);
                }
            }
        }
    }
}
