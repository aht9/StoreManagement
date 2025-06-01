namespace StoreManagement.Infrastructure.Repositories;

public class GenericRepository<T>(ApplicationDbContext ctx) : IGenericRepository<T>
    where T : BaseEntity, new()
{
    private readonly IFilterStrategy<T> _filterStrategy = new SoftDeleteFilterStrategy<T>();

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        var baseSpecification = new DefaultExpressionSpecification<T>(); 
        var filteredSpecification = _filterStrategy.ApplyFilter(baseSpecification);
        return await ctx.Set<T>().Where(filteredSpecification.ToExpression()).ToListAsync();
    }

    public async Task AddAsync(T entity)
        => await ctx.Set<T>().AddAsync(entity);

    public void Delete(T entity)
        => ctx.Set<T>().Remove(entity);

    public async Task<T> GetByIdAsync(int id)
        => await ctx.Set<T>().FindAsync(id);

    public void Update(T entity)
        => ctx.Set<T>().Update(entity);
}