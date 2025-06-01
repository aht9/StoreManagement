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
        Guid templateId,
        Dictionary<string, string> parameters,
        string? providerName = null,
        CancellationToken cancellationToken = default);

    Task<SmsDeliveryStatus> CheckDeliveryStatusAsync(
        Guid messageId,
        CancellationToken cancellationToken = default);

    Task<bool> RetryFailedMessageAsync(
        Guid messageId,
        string? providerName = null,
        CancellationToken cancellationToken = default);
}