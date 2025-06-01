namespace StoreManagement.Domain.Notifications;

public interface ISmsProvider
{
    string Name { get; }

    Task<SmsResult> SendAsync(SmsMessage message, CancellationToken cancellationToken = default);

    Task<SmsDeliveryStatus> CheckDeliveryStatusAsync(SmsMessage message, CancellationToken cancellationToken = default);

    bool CanHandleTemplate(SmsTemplate template);
}