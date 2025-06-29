namespace StoreManagement.WPF.ViewModels.Invoicing;

public class InvoicingViewModelFactory(IMediator mediator) : IInvoicingViewModelFactory
{
    public InvoicingViewModel Create(InvoiceType invoiceType, ISnackbarMessageQueue messageQueue)
    {
        return new InvoicingViewModel(invoiceType, mediator, messageQueue);
    }
}