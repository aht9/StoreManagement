namespace StoreManagement.WPF.ViewModels.Invoicing.FactoryHelper;

public interface IInvoiceListViewModelFactory
{
    /// <summary>
    /// یک نمونه از InvoiceListViewModel را بر اساس نوع فاکتور و با ارجاع به MainViewModel برای ناوبری می‌سازد.
    /// </summary>
    InvoiceListViewModel Create(InvoiceType invoiceType, MainViewModel mainViewModel);
}