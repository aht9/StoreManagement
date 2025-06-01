namespace StoreManagement.Domain.Aggregates.SmsModels;

public class SmsMessage : BaseEntity, IAggregateRoot
{
    public PhoneNumber PhoneNumber { get; private set; }
    public string Content { get; private set; }
    public SmsStatus Status { get; private set; }
    public string ProviderName { get; private set; }
    public string? TrackingCode { get; private set; }
    public DateTime? SentAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int RetryCount { get; private set; }
    public long? TemplateId { get; private set; }
    public SmsTemplate? Template { get; private set; }
    public string? ParametersJson { get; private set; } = string.Empty;

    private List<SmsLog> _logs = new List<SmsLog>();
    public IReadOnlyCollection<SmsLog> SmsLogs => _logs.AsReadOnly();


    [NotMapped]
    public Dictionary<string, string> Parameters
    {
        get
        {
            if (string.IsNullOrEmpty(ParametersJson))
                return new Dictionary<string, string>();

            return JsonSerializer.Deserialize<Dictionary<string, string>>(ParametersJson);
        }
        private set
        {
            ParametersJson = value != null && value.Count > 0
                ? JsonSerializer.Serialize(value)
                : null;
        }
    }

    public SmsMessage(
        PhoneNumber phoneNumber,
        string content,
        string providerName)
    {
        PhoneNumber = phoneNumber ?? throw new ArgumentNullException(nameof(phoneNumber));
        Content = content;
        ProviderName = providerName;
        Status = SmsStatus.Pending;
        RetryCount = 0;
        ParametersJson = string.Empty;
        Parameters = new Dictionary<string, string>();
    }


    public SmsMessage(
        string phoneNumber,
        string content,
        string providerName)
    {
        Content = content;
        ProviderName = providerName;
        Status = SmsStatus.Pending;
        RetryCount = 0;
        ParametersJson = string.Empty;
        Parameters = new Dictionary<string, string>();
        var phone = PhoneNumber.Create(phoneNumber);
        PhoneNumber = phone.IsSuccess ? phone.Value : throw new ArgumentException(phone.Error);
    }

    public SmsMessage(
        string phoneNumber,
        long templateId,
        Dictionary<string, string> parameters,
        string providerName)
    {
        
        TemplateId = templateId;
        Parameters = parameters ?? new Dictionary<string, string>();
        ProviderName = providerName;
        Status = SmsStatus.Pending;
        RetryCount = 0;
        Content = string.Empty; // Will be populated from template
        ParametersJson = string.Empty;

        var phone = PhoneNumber.Create(phoneNumber);
        PhoneNumber = phone.IsSuccess ? phone.Value : throw new ArgumentException(phone.Error);
    }

    public void SetTrackingCode(string trackingCode)
    {
        TrackingCode = trackingCode;
    }

    public void AddParameter(string key, string value)
    {
        var dict = Parameters;
        dict[key] = value;
        Parameters = dict; // این باعث به‌روزرسانی ParametersJson می‌شود
    }

    public string GetParameter(string key)
    {
        var dict = Parameters;
        return dict.TryGetValue(key, out var value) ? value : null;
    }

    public void MarkAsSent()
    {
        Status = SmsStatus.Sent;
        SentAt = DateTime.UtcNow;
    }

    public void MarkAsDelivered()
    {
        Status = SmsStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string errorMessage)
    {
        Status = SmsStatus.Failed;
        ErrorMessage = errorMessage;
    }

    public void IncrementRetryCount()
    {
        RetryCount++;
    }

    public bool CanRetry(int maxRetries)
    {
        return Status == SmsStatus.Failed && RetryCount < maxRetries;
    }

    public void SetContent(string content)
    {
        Content = content;
    }


    public void AddLog(SmsLog log)
    {
        if (log == null) throw new ArgumentNullException(nameof(log));
        _logs.Add(log);
    }

    public void ClearLogs()
    {
        _logs.Clear();
    }
}