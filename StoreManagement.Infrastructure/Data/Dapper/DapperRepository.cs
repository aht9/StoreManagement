namespace StoreManagement.Infrastructure.Data.Dapper;

public class DapperRepository(DapperContext context) : IDapperRepository
{
    private readonly DapperContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null)
    {
        using var conn = _context.CreateConnection();
        return await conn.QueryAsync<T>(sql, param);
    }

    public async Task<T> QuerySingleAsync<T>(string sql, object param = null)
    {
        using var conn = _context.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<T>(sql, param);
    }

    public async Task<int> ExecuteAsync(string sql, object param = null)
    {
        using var conn = _context.CreateConnection();
        return await conn.ExecuteAsync(sql, param);
    }
}