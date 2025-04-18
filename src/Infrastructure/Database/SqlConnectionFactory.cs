using Microsoft.Data.SqlClient;

public class SqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("Default");
    }

    public SqlConnection CreateConnection() => new SqlConnection(_connectionString);
}