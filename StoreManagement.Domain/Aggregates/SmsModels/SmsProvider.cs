namespace StoreManagement.Domain.Aggregates.SmsModels;

public class SmsProvider : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; }
    public string TypeName { get; private set; }
    public bool IsActive { get; private set; }
    public int Priority { get; private set; }
    public string? SettingsJson { get; private set; }

    [NotMapped]
    public Dictionary<string, string> Settings
    {
        get
        {
            if (string.IsNullOrEmpty(SettingsJson))
                return new Dictionary<string, string>();

            return JsonSerializer.Deserialize<Dictionary<string, string>>(SettingsJson);
        }
        private set
        {
            SettingsJson = value != null && value.Count > 0
                ? JsonSerializer.Serialize(value)
                : null;
        }
    }

    // For EF Core
    private SmsProvider() { }

    public SmsProvider(string name, string typeName, int priority)
    {
        Name = name;
        TypeName = typeName;
        Priority = priority;
        IsActive = true;
        Settings = new Dictionary<string, string>();
    }


    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void UpdatePriority(int priority)
    {
        Priority = priority;
    }

    public void AddSetting(string key, string value)
    {
        var dict = Settings;
        dict[key] = value;
        Settings = dict;
    }

    public void RemoveSetting(string key)
    {
        var dict = Settings;
        if (dict.ContainsKey(key))
        {
            dict.Remove(key);
            Settings = dict;
        }
    }

    public string? GetSetting(string key)
    {
        var dict = Settings;
        return dict.TryGetValue(key, out var value) ? value : null;
    }

    public bool HasSetting(string key)
    {
        return Settings.ContainsKey(key);
    }

    public IReadOnlyDictionary<string, string> GetAllSettings()
    {
        return new Dictionary<string, string>(Settings);
    }

    public void ClearSettings()
    {
        Settings = new Dictionary<string, string>();
    }
}