namespace StoreManagement.WPF.ViewModels.Invoicing.FactoryHelper;

public class InvoicingViewModelFactory(IMediator mediator) : IInvoicingViewModelFactory
{
    public InvoicingViewModel Create(InvoiceType invoiceType, ISnackbarMessageQueue messageQueue)
    {
        return new InvoicingViewModel(invoiceType, mediator, messageQueue);
    }
}