namespace StoreManagement.Infrastructure.Notifications;

public class SmsService : ISmsService
{
    public Task<SmsMessage> SendAsync(string phoneNumber, string content, string? providerName = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<SmsMessage> SendWithTemplateAsync(string phoneNumber, Guid templateId, Dictionary<string, string> parameters, string? providerName = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<SmsDeliveryStatus> CheckDeliveryStatusAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RetryFailedMessageAsync(Guid messageId, string? providerName = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}