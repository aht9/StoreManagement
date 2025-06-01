namespace StoreManagement.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity, new()
{
    private readonly ApplicationDbContext _ctx;
    public GenericRepository(ApplicationDbContext ctx)
        => _ctx = ctx;

    public async Task AddAsync(T entity)
        => await _ctx.Set<T>().AddAsync(entity);

    public void Delete(T entity)
        => _ctx.Set<T>().Remove(entity);

    public async Task<T> GetByIdAsync(int id)
        => await _ctx.Set<T>().FindAsync(id);

    public void Update(T entity)
        => _ctx.Set<T>().Update(entity);
}