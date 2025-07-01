namespace StoreManagement.WPF.ViewModels.Invoicing.FactoryHelper;

public class InvoiceListViewModelFactory(IMediator mediator, ISnackbarMessageQueue snackbarMessageQueue) : IInvoiceListViewModelFactory
{

    public InvoiceListViewModel Create(InvoiceType invoiceType, MainViewModel mainViewModel)
    {
        return new InvoiceListViewModel(mediator, mainViewModel,snackbarMessageQueue, invoiceType);
    }
}