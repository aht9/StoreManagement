namespace StoreManagement.Infrastructure.Data.Dapper;

public class DapperRepository(DapperContext context) : IDapperRepository
{
    private readonly DapperContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(sql, param, transaction, cancellationToken: cancellationToken);
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<T>(command);
    }

    public async Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(sql, param, transaction, cancellationToken: cancellationToken);
        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<T>(command);
    }

    public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(sql, param, transaction, cancellationToken: cancellationToken);
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<T>(command);
    }

    public async Task<int> ExecuteAsync(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(sql, param, transaction, cancellationToken: cancellationToken);
        using var connection = _context.CreateConnection();
        return await connection.ExecuteAsync(command);
    }

    public async Task<SqlMapper.GridReader> QueryMultipleAsync(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(sql, param, transaction, cancellationToken: cancellationToken);
        var connection = _context.CreateConnection();
        return await connection.QueryMultipleAsync(command);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}