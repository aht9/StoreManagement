namespace StoreManagement.Domain.Common.Interface;

public interface IDapperRepository
{
    /// <summary>
    /// Executes a query and returns a list of results asynchronously.
    /// </summary>
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a query and returns a single result asynchronously.
    /// </summary>
    Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a command (like INSERT, UPDATE, DELETE) and returns the number of affected rows asynchronously.
    /// </summary>
    Task<int> ExecuteAsync(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a query and returns a first result asynchronously.
    /// </summary>
    Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);


    /// <summary>
    /// Executes multiple queries within the same command and returns a grid reader to process multiple result sets asynchronously.
    /// </summary>
    Task<SqlMapper.GridReader> QueryMultipleAsync(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
}