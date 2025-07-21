using StoreManagement.WPF.ViewModels.Invoicing;

namespace StoreManagement.WPF.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPublicServiceCollection(this IServiceCollection services)
    {
        // Register Serilog's ILogger for injection
        services.AddSingleton(Log.Logger);

        // Register MediatR PipelineBehavior 
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

        // Register MediatR for handling commands and queries
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateCustomerCommand).Assembly));


        // Register AutoMapper

        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<StoreMappingProfile>();
        }, typeof(CreateCustomerCommand).Assembly);

        return services;
    }

    public static IServiceCollection AddViewModelServiceCollection(this IServiceCollection services)
    {
        services.AddSingleton<ISnackbarMessageQueue, SnackbarMessageQueue>();


        // ViewModels (transient or singleton based on need)
        services.AddSingleton<MainViewModel>();
        services.AddTransient<CustomerManagementViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<BankAccountManagementViewModel>();
        services.AddTransient<ProductManagementViewModel>();
        services.AddTransient<StoreManagementViewModel>();
        services.AddTransient<ProductCategoryManagementViewModel>();
        services.AddTransient<InventoryManagementViewModel>();

        services.AddSingleton<IInvoicingViewModelFactory, InvoicingViewModelFactory>(); 
        services.AddSingleton<IInvoiceListViewModelFactory, InvoiceListViewModelFactory>(); 
        services.AddSingleton<IEditInvoiceViewModelFactory, EditInvoiceViewModelFactory>();
        services.AddSingleton<IInstallmentManagementViewModelFactory, InstallmentManagementViewModelFactory>();


        services.AddTransient<AddCustomerViewModel>();
        services.AddTransient<EditCustomerViewModel>();
        services.AddTransient<AddBankAccountViewModel>();
        services.AddTransient<EditBankAccountViewModel>();
        services.AddTransient<AddTransactionViewModel>();

        services.AddTransient<AddProductViewModel>();
        services.AddTransient<EditProductViewModel>();
        services.AddTransient<AddProductVariantViewModel>();
        services.AddTransient<EditProductVariantViewModel>();
        services.AddTransient<AddProductCategoryViewModel>();

        services.AddTransient<AddStoreViewModel>();
        services.AddTransient<EditStoreViewModel>();

        services.AddTransient<InvoiceItemViewModel>();
        services.AddTransient<PaymentDialogViewModel>();
        services.AddTransient<AddPartyDialogViewModel>();
        services.AddTransient<AddProductDialogViewModel>();
        services.AddTransient<ConfirmationDialogViewModel>();
        services.AddTransient<EditInvoiceViewModel>();
        services.AddTransient<InstallmentManagementViewModel>();
        services.AddTransient<PayInstallmentDialogViewModel>();
        services.AddTransient<PaymentDialogViewModel>();
        services.AddTransient<SelectPartyDialogViewModel>();

        return services;
    }
}