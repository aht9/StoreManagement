using StoreManagement.Domain.Specifications;

namespace StoreManagement.Domain.Common.Interface;

public interface IGenericRepository<T> where T : BaseEntity
{
    IUnitOfWork UnitOfWork { get; }

    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

    Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    Task SoftDeleteAsync(T entity, CancellationToken cancellationToken = default);

    Task<T?> GetByIdForUpdateAsync(long id, CancellationToken cancellationToken = default);

    Task<List<T?>> GetAllAsync();

    Task<T?> GetByIdAsync(long id, IncludeSpecification<T> includes, CancellationToken cancellationToken = default);

    Task<T?> FirstOrDefaultAsync(IExpressionSpecification<T?> specification,
        CancellationToken cancellationToken = default);

    Task<T?> FirstOrDefaultAsync(IExpressionSpecification<T> specification, IncludeSpecification<T> includes,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> ListAllAsync(CancellationToken cancellationToken = default);

    Task<List<T?>> ListAsync(IExpressionSpecification<T> specification,
        IncludeSpecification<T> includes, CancellationToken cancellationToken = default);

    Task<List<T?>> ListAsync(IExpressionSpecification<T> specification,
        OrderBySpecification<T, object> orderBy,
        CancellationToken cancellationToken = default);

    Task<List<T?>> ListAsync(IExpressionSpecification<T> specification,
        OrderBySpecification<T, object> orderBy,
        IncludeSpecification<T> includes,
        CancellationToken cancellationToken = default);

    Task<List<T?>> ListAsync(IExpressionSpecification<T> specification,
        PaginationSpecification<T> pagination,
        CancellationToken cancellationToken = default);

    Task<List<T?>> ListAsync(IExpressionSpecification<T> specification,
        PaginationSpecification<T> pagination,
        OrderBySpecification<T, object> orderBy,
        CancellationToken cancellationToken = default);

    Task<List<T?>> ListAsync(IExpressionSpecification<T> specification,
        PaginationSpecification<T> pagination,
        OrderBySpecification<T, object> orderBy,
        IncludeSpecification<T> includes,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(IExpressionSpecification<T> specification, CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(IExpressionSpecification<T> specification, CancellationToken cancellationToken = default);

}