namespace StoreManagement.WPF.ViewModels.Invoicing.FactoryHelper;

public class InstallmentManagementViewModelFactory(IMediator mediator, ISnackbarMessageQueue snackbarMessageQueue)
    : IInstallmentManagementViewModelFactory
{
    public InstallmentManagementViewModel Create(long invoiceId, InvoiceType invoiceType)
    {
        return new InstallmentManagementViewModel(mediator, snackbarMessageQueue, invoiceId, invoiceType);
    }
}