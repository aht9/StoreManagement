namespace StoreManagement.Domain.Notifications;

public interface ISmsService
{
    Task<SmsMessage> SendAsync(
        string phoneNumber,
        string content,
        string? providerName = null,
        CancellationToken cancellationToken = default);

    Task<SmsMessage> SendWithTemplateAsync(
        string phoneNumber,
        long templateId,
        Dictionary<string, string> parameters,
        string? providerName = null,
        CancellationToken cancellationToken = default);

    Task<SmsDeliveryStatus> CheckDeliveryStatusAsync(
        long messageId,
        CancellationToken cancellationToken = default);

    Task<bool> RetryFailedMessageAsync(
        long messageId,
        string? providerName = null,
        CancellationToken cancellationToken = default);
}