namespace StoreManagement.Infrastructure.Notifications;

public class SmsProviderFactory(IEnumerable<ISmsProvider> providers, IGenericRepository<SmsProvider> providerRepository)
    : ISmsProviderFactory
{
    private readonly IEnumerable<ISmsProvider> _providers = providers;
    private readonly IGenericRepository<SmsProvider> _providerRepository = providerRepository;

    public ISmsProvider GetProvider(string providerName)
    {
        var provider = _providers.FirstOrDefault(p => p.Name.Equals(providerName, StringComparison.OrdinalIgnoreCase));

        if (provider == null)
        {
            throw new ApplicationException($"SMS provider '{providerName}' not found.");
        }

        return provider;
    }

    public ISmsProvider GetDefaultProvider()
    {
        var activeProviders = _providerRepository.ListAsync(
                new SmsProviderByActiveSpecification(),
                new OrderBySpecification<SmsProvider, object>(p => p.Priority),
                CancellationToken.None)
            .GetAwaiter().GetResult();
        var defaultProviderName = activeProviders.FirstOrDefault()?.Name ?? "Development";
        return GetProvider(defaultProviderName);
    }

    public IEnumerable<ISmsProvider> GetAllProviders()
    {
        return _providers;
    }
}