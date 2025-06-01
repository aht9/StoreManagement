namespace StoreManagement.Domain.Aggregates.SmsModels;

public class SmsTemplate : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; }
    public string Content { get; private set; }
    public bool IsActive { get; private set; }
    public string? DefaultParametersJson { get; private set; }
    
    private List<SmsMessage> _messages = new List<SmsMessage>();
    public IReadOnlyCollection<SmsMessage> Messages => _messages.AsReadOnly();


    [NotMapped]
    public Dictionary<string, string> DefaultParameters
    {
        get
        {
            if (string.IsNullOrEmpty(DefaultParametersJson))
                return new Dictionary<string, string>();

            return JsonSerializer.Deserialize<Dictionary<string, string>>(DefaultParametersJson);
        }
        private set
        {
            DefaultParametersJson = value != null && value.Count > 0
                ? JsonSerializer.Serialize(value)
                : null;
        }
    }

    // For EF Core
    private SmsTemplate()
    {
    }

    public SmsTemplate(string name, string content)
    {
        Name = name;
        Content = content;
        IsActive = true;
        DefaultParameters = new Dictionary<string, string>();
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void UpdateContent(string content)
    {
        Content = content;
    }

    public void AddDefaultParameter(string key, string value)
    {
        var dict = DefaultParameters;
        dict[key] = value;
        DefaultParameters = dict;
    }

    public void RemoveDefaultParameter(string key)
    {
        var dict = DefaultParameters;
        if (dict.ContainsKey(key))
        {
            dict.Remove(key);
            DefaultParameters = dict;
        }
    }

    public string? GetDefaultParameter(string key)
    {
        var dict = DefaultParameters;
        return dict.TryGetValue(key, out var value) ? value : null;
    }

    public bool HasDefaultParameter(string key)
    {
        return DefaultParameters.ContainsKey(key);
    }

    public IReadOnlyDictionary<string, string> GetAllDefaultParameters()
    {
        return new Dictionary<string, string>(DefaultParameters);
    }

    public void ClearDefaultParameters()
    {
        DefaultParameters = new Dictionary<string, string>();
    }

    public string FormatContent(Dictionary<string, string> parameters)
    {
        var allParameters = new Dictionary<string, string>(DefaultParameters);

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                allParameters[param.Key] = param.Value;
            }
        }

        string formattedContent = Content;

        foreach (var param in allParameters)
        {
            formattedContent = formattedContent.Replace($"{{{param.Key}}}", param.Value);
        }

        return formattedContent;
    }

    /// <summary>
    /// ساخت یک نمونه از قالب با پارامترهای مشخص شده
    /// </summary>
    public string CreateInstance(Dictionary<string, string> parameters = null)
    {
        return FormatContent(parameters);
    }
}