namespace StoreManagement.WPF.ViewModels.Invoicing;

public interface IInvoicingViewModelFactory
{
    InvoicingViewModel Create(InvoiceType invoiceType, ISnackbarMessageQueue messageQueue);
}