namespace StoreManagement.Domain.Common.Interface;

public interface IDapperRepository
{
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null);
    Task<T> QuerySingleAsync<T>(string sql, object param = null);
    Task<int> ExecuteAsync(string sql, object param = null);
}