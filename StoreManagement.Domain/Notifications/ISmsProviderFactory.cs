
namespace StoreManagement.Domain.Notifications;

public interface ISmsProviderFactory
{
    ISmsProvider GetProvider(string providerName);

    ISmsProvider GetDefaultProvider();

    IEnumerable<ISmsProvider> GetAllProviders();
}