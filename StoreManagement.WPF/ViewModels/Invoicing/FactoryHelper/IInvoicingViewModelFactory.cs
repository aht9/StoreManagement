namespace StoreManagement.WPF.ViewModels.Invoicing.FactoryHelper;

public interface IInvoicingViewModelFactory
{
    InvoicingViewModel Create(InvoiceType invoiceType, ISnackbarMessageQueue messageQueue);
}