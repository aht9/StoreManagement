namespace StoreManagement.Domain.Common.Interface;

public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));

    Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default(CancellationToken));
}