namespace StoreManagement.Infrastructure.Notifications;

public class SmsProviderFactory : ISmsProviderFactory
{
    public ISmsProvider GetProvider(string providerName)
    {
        throw new NotImplementedException();
    }

    public ISmsProvider GetDefaultProvider()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ISmsProvider> GetAllProviders()
    {
        throw new NotImplementedException();
    }
}