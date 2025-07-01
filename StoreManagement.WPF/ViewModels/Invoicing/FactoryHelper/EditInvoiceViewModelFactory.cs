namespace StoreManagement.WPF.ViewModels.Invoicing.FactoryHelper;

public class EditInvoiceViewModelFactory(IMediator mediator, ISnackbarMessageQueue snackbarMessageQueue)
    : IEditInvoiceViewModelFactory
{
    public EditInvoiceViewModel Create(long invoiceId, InvoiceType invoiceType, MainViewModel mainViewModel)
    {
        return new EditInvoiceViewModel(mediator, snackbarMessageQueue, mainViewModel, invoiceId, invoiceType);
    }
}