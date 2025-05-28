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
    }
    
    public void Restore()
    {
        IsDeleted = false;
    }

    public bool IsTransient()
    {
        return this.Id  == default(long);
    }
    
    // Domain events 
    private List<INotification> _domainEvents = new List<INotification>();
    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();
    
    public void AddDomainEvent(INotification eventItem)
    {
        _domainEvents = _domainEvents ?? new List<INotification>();
        _domainEvents.Add(eventItem);
    }
    
    public void RemoveDomainEvent(INotification eventItem)
    {
        _domainEvents?.Remove(eventItem);
    }
    
    public void ClearDomainEvents()
    {
        _domainEvents?.Clear();
    }

}