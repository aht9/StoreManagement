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

        /// 5) MediatR برای Publish رویدادهای دامنه
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationDbContext).Assembly));

        return services;
    }
}