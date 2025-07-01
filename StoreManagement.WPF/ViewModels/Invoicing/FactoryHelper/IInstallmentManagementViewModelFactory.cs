namespace StoreManagement.WPF.ViewModels.Invoicing.FactoryHelper;

public interface IInstallmentManagementViewModelFactory
{
    InstallmentManagementViewModel Create(long invoiceId, InvoiceType invoiceType);
}