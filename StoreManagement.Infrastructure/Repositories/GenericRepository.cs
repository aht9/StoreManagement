namespace StoreManagement.Infrastructure.Repositories;

public class GenericRepository<T>(ApplicationDbContext _dbContext) : IGenericRepository<T>
    where T : BaseEntity
{
    private readonly IFilterStrategy<T?> _filterStrategy = new SoftDeleteFilterStrategy<T?>();
    public IUnitOfWork UnitOfWork => _dbContext;

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<T>().AddAsync(entity, cancellationToken);
        return entity;
    }

    public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<T>().AddRangeAsync(entities, cancellationToken);
        return entities;
    }

    public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Entry(entity).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public virtual Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
        }

        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<T>().Remove(entity);
        return Task.CompletedTask;
    }

    public virtual Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<T>().RemoveRange(entities);
        return Task.CompletedTask;
    }

    public virtual Task SoftDeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.MarkAsDeleted();
        _dbContext.Entry(entity).State = EntityState.Modified;
        return Task.CompletedTask;
    }


    public virtual async Task<T?> GetByIdForUpdateAsync(long id, CancellationToken cancellationToken = default)
    {
        // Use tracking for entities that will be updated
        return await _dbContext.Set<T>()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }


    public async Task<List<T?>> GetAllAsync()
    {
        var baseSpecification = new DefaultExpressionSpecification<T?>();
        var filteredSpecification = _filterStrategy.ApplyFilter(baseSpecification);
        return await _dbContext.Set<T>().Where(filteredSpecification.ToExpression()).ToListAsync();
    }

    public virtual async Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        IQueryable<T?> query = _dbContext.Set<T>().AsQueryable();

        return await query.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public virtual async Task<T?> GetByIdAsync(long id, IncludeSpecification<T> includes, CancellationToken cancellationToken = default)
    {
        IQueryable<T?> query = _dbContext.Set<T>().AsQueryable();
        query = ApplyIncludes(query, includes);

        return await query.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public virtual async Task<T?> FirstOrDefaultAsync(IExpressionSpecification<T?> specification, CancellationToken cancellationToken = default)
    {
        var filteredSpec = _filterStrategy.ApplyFilter(specification);

        return await _dbContext.Set<T>()
            .AsQueryable()
            .Where(filteredSpec.ToExpression())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<T?> FirstOrDefaultAsync(IExpressionSpecification<T> specification, IncludeSpecification<T> includes, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<T>().AsQueryable();
        query = ApplyIncludes(query, includes)!;

        var filteredSpec = _filterStrategy.ApplyFilter(specification!);

        return await query
            .Where(filteredSpec.ToExpression())
            .FirstOrDefaultAsync(cancellationToken);
    }


    public virtual async Task<IReadOnlyList<T>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<T>()
            .ToListAsync(cancellationToken);
    }

    public virtual async Task<List<T?>> ListAsync(IExpressionSpecification<T> specification,
        IncludeSpecification<T> includes, CancellationToken cancellationToken = default)
    {
        IQueryable<T?> query = _dbContext.Set<T>().AsQueryable();
        query = ApplyIncludes(query, includes);

        var filteredSpec = _filterStrategy.ApplyFilter(specification!);
        return await query
            .Where(filteredSpec.ToExpression())
            .ToListAsync(cancellationToken);
    }


    public virtual async Task<List<T?>> ListAsync(IExpressionSpecification<T> specification,
        OrderBySpecification<T, object> orderBy,
        CancellationToken cancellationToken = default)
    {
        var filteredSpec = _filterStrategy.ApplyFilter(specification!);
        var query = _dbContext.Set<T>()
            .Where(filteredSpec.ToExpression());

        query = ApplyOrderBy(query, orderBy);

        return await query.ToListAsync(cancellationToken);
    }

    public virtual async Task<List<T?>> ListAsync(IExpressionSpecification<T> specification,
        OrderBySpecification<T, object> orderBy,
        IncludeSpecification<T> includes,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T?> query = _dbContext.Set<T>().AsQueryable();
        query = ApplyIncludes(query, includes);

        var filteredSpec = _filterStrategy.ApplyFilter(specification!);

        query = query.Where(filteredSpec.ToExpression())!;
        query = ApplyOrderBy(query, orderBy);

        return await query.ToListAsync(cancellationToken);
    }

    //Use paginate results
    public virtual async Task<List<T?>> ListAsync(IExpressionSpecification<T> specification,
        PaginationSpecification<T> pagination,
        CancellationToken cancellationToken = default)
    {
        var filteredSpec = _filterStrategy.ApplyFilter(specification!);
        return await _dbContext.Set<T>()
            .Where(filteredSpec.ToExpression())
            .Skip(pagination.Skip)
            .Take(pagination.Take)
            .ToListAsync(cancellationToken);
    }

    public virtual async Task<List<T?>> ListAsync(IExpressionSpecification<T> specification,
        PaginationSpecification<T> pagination,
        OrderBySpecification<T, object> orderBy,
        CancellationToken cancellationToken = default)
    {
        var filteredSpec = _filterStrategy.ApplyFilter(specification!);
        var query = _dbContext.Set<T>()
            .Where(filteredSpec.ToExpression());

        query = ApplyOrderBy(query, orderBy);

        return await query
            .Skip(pagination.Skip)
            .Take(pagination.Take)
            .ToListAsync(cancellationToken);
    }

    public virtual async Task<List<T?>> ListAsync(IExpressionSpecification<T> specification,
        PaginationSpecification<T> pagination,
        OrderBySpecification<T, object> orderBy,
        IncludeSpecification<T> includes,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T?> query = _dbContext.Set<T>().AsQueryable();
        query = ApplyIncludes(query, includes);

        var filteredSpec = _filterStrategy.ApplyFilter(specification!);
        query = query.Where(filteredSpec.ToExpression());
        query = ApplyOrderBy(query, orderBy);

        return await query
            .Skip(pagination.Skip)
            .Take(pagination.Take)
            .ToListAsync(cancellationToken);
    }

    public virtual async Task<int> CountAsync(IExpressionSpecification<T> specification, CancellationToken cancellationToken = default)
    {
        var filteredSpec = _filterStrategy.ApplyFilter(specification);
        return await _dbContext.Set<T>()
            .Where(filteredSpec.ToExpression())
            .CountAsync(cancellationToken);
    }

    public virtual async Task<bool> AnyAsync(IExpressionSpecification<T> specification, CancellationToken cancellationToken = default)
    {
        var filteredSpec = _filterStrategy.ApplyFilter(specification);
        return await _dbContext.Set<T>()
            .AnyAsync(filteredSpec.ToExpression(), cancellationToken);
    }







    // Helper methods
    protected virtual IQueryable<T?> ApplyIncludes(IQueryable<T?> query, IncludeSpecification<T> includes)
    {
        if (includes == null)
            return query;

        foreach (var include in includes.Includes)
        {
            query = query.Include(include);
        }

        foreach (var includeString in includes.IncludeStrings)
        {
            query = query.Include(includeString);
        }

        return query;
    }

    protected virtual IQueryable<T> ApplyOrderBy(IQueryable<T?> query, OrderBySpecification<T, object> orderBy)
    {
        if (orderBy == null)
            return query;

        return orderBy.Descending
            ? query.OrderByDescending(orderBy.OrderByExpression)
            : query.OrderBy(orderBy.OrderByExpression);
    }
}