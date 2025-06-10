using StoreManagement.Infrastructure.Notifications;

namespace StoreManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // 1) DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // 2) Dapper Context (Singleton)
        services.AddSingleton<DapperContext>(sp => new DapperContext(config));

        // 3) Dapper
        services.AddSingleton<DapperContext>();
        services.AddScoped<IDapperRepository, DapperRepository>();

        // 4) UnitOfWork و GenericRepository
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        ///// 5) MediatR برای Publish رویدادهای دامنه
        //services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationDbContext).Assembly));

        // 6) ServiceCollection Extensions
        services.AddScoped<ISmsProviderFactory, SmsProviderFactory>();
        services.AddScoped<ISmsService, SmsService>();

        // 7) Repositories
        services.AddScoped<IGenericRepository<Customer>, GenericRepository<Customer>>();
        services.AddScoped<IGenericRepository<Store>, GenericRepository<Store>>();
        services.AddScoped<IGenericRepository<Product>, GenericRepository<Product>>();
        services.AddScoped<IGenericRepository<ProductVariant>, GenericRepository<ProductVariant>>();
        services.AddScoped<IGenericRepository<PurchaseInvoice>, GenericRepository<PurchaseInvoice>>();
        services.AddScoped<IGenericRepository<PurchaseInvoiceItem>, GenericRepository<PurchaseInvoiceItem>>();
        services.AddScoped<IGenericRepository<SalesInvoice>, GenericRepository<SalesInvoice>>();
        services.AddScoped<IGenericRepository<SalesInvoiceItem>, GenericRepository<SalesInvoiceItem>>();
        services.AddScoped<IGenericRepository<InventoryTransaction>, GenericRepository<InventoryTransaction>>();
        services.AddScoped<IGenericRepository<BankAccount>, GenericRepository<BankAccount>>();
        services.AddScoped<IGenericRepository<FinancialTransaction>, GenericRepository<FinancialTransaction>>();
        services.AddScoped<IGenericRepository<Installment>, GenericRepository<Installment>>();
        services.AddScoped<IGenericRepository<ProductCategory>, GenericRepository<ProductCategory>>();
        services.AddScoped<IGenericRepository<SmsLog>, GenericRepository<SmsLog>>();
        services.AddScoped<IGenericRepository<SmsMessage>, GenericRepository<SmsMessage>>();
        services.AddScoped<IGenericRepository<SmsTemplate>, GenericRepository<SmsTemplate>>();
        services.AddScoped<IGenericRepository<SmsProvider>, GenericRepository<SmsProvider>>();

        return services;
    }
}
