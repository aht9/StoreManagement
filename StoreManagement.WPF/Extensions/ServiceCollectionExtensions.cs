namespace StoreManagement.WPF.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPublicServiceCollection(this IServiceCollection services)
    {
        // Register Serilog's ILogger for injection
        services.AddSingleton(Log.Logger);

        // Register MediatR PipelineBehavior 
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

        return services;
    }

    public static IServiceCollection AddViewModelServiceCollection(this IServiceCollection services)
    {
        // ViewModels (transient or singleton based on need)
        services.AddSingleton<MainViewModel>();
        services.AddTransient<CustomerManagementViewModel>();
        services.AddTransient<DashboardViewModel>();
        return services;
    }
}