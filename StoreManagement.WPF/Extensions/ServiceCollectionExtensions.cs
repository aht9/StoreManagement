using StoreManagement.WPF.Views;

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

        return services;
    }

    public static IServiceCollection AddViewModelServiceCollection(this IServiceCollection services)
    {
        // ViewModels (transient or singleton based on need)
        services.AddSingleton<MainViewModel>();
        services.AddTransient<CustomerManagementViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<BankAccountManagementViewModel>();
        services.AddTransient<ProductManagementViewModel>();

        services.AddTransient<AddCustomerViewModel>();
        services.AddTransient<EditCustomerViewModel>();
        services.AddTransient<AddBankAccountViewModel>();
        services.AddTransient<EditBankAccountViewModel>();
        services.AddTransient<AddTransactionViewModel>();

        services.AddTransient<AddProductView>();
        services.AddTransient<EditProductView>();
        services.AddTransient<AddProductVariantView>();
        services.AddTransient<EditProductVariantView>();

        return services;
    }
}