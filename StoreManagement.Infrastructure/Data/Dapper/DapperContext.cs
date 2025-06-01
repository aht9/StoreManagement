namespace StoreManagement.Infrastructure.Data.Dapper;
public class DapperContext
{
    private readonly string _connectionString;
    public DapperContext(IConfiguration config)
        => _connectionString = config.GetConnectionString("DefaultConnection");

    public IDbConnection CreateConnection()
        => new SqlConnection(_connectionString);
}