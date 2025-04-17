using Dapper;
using Microsoft.Data.SqlClient;


public class ProductRepository : IProductRepository
{
    private readonly SqlConnection _connection;

    public ProductRepository(SqlConnection connection)
    {
        _connection = connection;
    }

    public async Task<List<Product>> GetByCategoryAsync(string category)
    {
        const string sql = """
            SELECT 
                Id, Name, Description, Price, Category
            FROM Products
            WHERE Category = @Category
            """;

        return (await _connection.QueryAsync<Product>(sql, new { Category = category })).ToList();
    }

    public async Task<Product> GetByIdAsync(int id)
    {
        const string sql = """
            SELECT 
                Id, Name, Description, Price, Category
            FROM Products
            WHERE Id = @Id
            """;

        return await _connection.QueryFirstOrDefaultAsync<Product>(sql, new { Id = id });
    }

    public async Task<List<Product>> SearchAsync(string query)
    {
        const string sql = """
            SELECT 
                Id, Name, Description, Price, Category
            FROM Products
            WHERE Name LIKE '%' + @Query + '%'
               OR Description LIKE '%' + @Query + '%'
            """;

        return (await _connection.QueryAsync<Product>(sql, new { Query = query })).ToList();
    }
}