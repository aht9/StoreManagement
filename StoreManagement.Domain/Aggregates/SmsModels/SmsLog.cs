namespace StoreManagement.Domain.Aggregates.SmsModels;

public class SmsLog : BaseEntity, IAggregateRoot
{
    public long SmsMessageId { get; private set; }
    public string ProviderName { get; private set; }
    public SmsLogType LogType { get; private set; }
    public string? Data { get; private set; }

    public SmsMessage SmsMessage { get; private set; }

    private SmsLog() { }

    public SmsLog(long smsMessageId, string providerName, SmsLogType logType, string? data = null)
    {
        SmsMessageId = smsMessageId;
        ProviderName = providerName;
        LogType = logType;
        Data = data;
    }
}