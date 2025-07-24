namespace StoreManagement.WPF.Views.Invoicing;

public partial class PrintPreviewWindow : Window
{
    public PrintPreviewWindow()
    {
        InitializeComponent();
    }
    
    private void PrintButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                printDialog.PrintVisual(PrintableContent, "Invoice");
            }
        }
        catch (System.Exception)
        {
            MessageBox.Show("خطایی در فرآیند چاپ رخ داد.", "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}