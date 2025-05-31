namespace StoreManagement.Domain.Common;

/// <summary>
/// Represents the base class for all entities in the domain.
/// Provides common functionality such as identity management, 
/// soft deletion, domain event handling, and transient state checking.
/// </summary>
public class BaseEntity
{
    private long _id;

    public virtual long Id
    {
        get => _id;
        set => _id = value;
    }

    public bool IsDeleted { get; private set; } = false;

    public void MarkAsDeleted()
    {
        IsDeleted = true;
        UpdateTimestamp();
    }

    public void Restore()
    {
        IsDeleted = false;
        UpdateTimestamp();
    }

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    private void UpdateTimestamp()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsTransient()
    {
        return this.Id == default(long);
    }

    // Domain events 
    private List<INotification> _domainEvents = new List<INotification>();
    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(INotification eventItem)
    {
        _domainEvents = _domainEvents ?? new List<INotification>();
        _domainEvents.Add(eventItem);
        UpdateTimestamp();
    }

    public void RemoveDomainEvent(INotification eventItem)
    {
        _domainEvents?.Remove(eventItem);
        UpdateTimestamp();
    }

    public void ClearDomainEvents()
    {
        _domainEvents?.Clear();
        UpdateTimestamp();
    }

    // model behavior
    public void UpdateEntity(Action<BaseEntity>? updateAction = null)
    {
        if (updateAction != null)
        {
            updateAction(this);
        }
        UpdateTimestamp();
    }

    public void ValidateEntity(Func<BaseEntity, bool> validationLogic)
    {
        if (!validationLogic(this))
        {
            throw new InvalidOperationException("Entity validation failed.");
        }
    }


}