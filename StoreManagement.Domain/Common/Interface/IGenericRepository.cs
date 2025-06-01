namespace StoreManagement.Domain.Common.Interface;

public interface IGenericRepository<T> where T : BaseEntity 
{
    Task<T> GetByIdAsync(int id);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}