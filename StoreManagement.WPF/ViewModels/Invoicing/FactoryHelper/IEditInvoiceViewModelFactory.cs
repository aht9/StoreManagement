namespace StoreManagement.WPF.ViewModels.Invoicing.FactoryHelper;

public interface IEditInvoiceViewModelFactory
{
    EditInvoiceViewModel Create(long invoiceId, InvoiceType invoiceType, MainViewModel mainViewModel);
}