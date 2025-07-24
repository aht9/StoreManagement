using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StoreManagement.WPF.Views.Shared
{
    /// <summary>
    /// Interaction logic for TreeComboBox.xaml
    /// </summary>
    public partial class TreeComboBox : UserControl
    {
        public TreeComboBox()
        {
            InitializeComponent();
        }


        private void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem item && item.DataContext is ProductCategoryTreeDto selectedDto)
            {
                if (DataContext is TreeComboBoxViewModel viewModel)
                {
                    viewModel.SelectCategory(selectedDto);
                    e.Handled = true;
                }
            }
        }
    }
}
