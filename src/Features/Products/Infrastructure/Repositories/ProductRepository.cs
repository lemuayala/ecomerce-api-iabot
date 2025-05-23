using Dapper;
using EcomerceAI.Api.Features.Products.Domain.Models;
using Microsoft.Data.SqlClient;

public class ProductRepository : IProductRepository
{
    private readonly SqlConnection _connection;

    public ProductRepository(SqlConnection connection)
    {
        _connection = connection;
    }

    public async Task<Product> GetByIdAsync(int id)
    {
        const string sql = """
        SELECT Id, Name, Description, Price, Category, ImageUrl, Tags, Metadata
        FROM Products
        WHERE Id = @Id
        """;

        var product = await _connection.QueryFirstOrDefaultAsync<Product>(sql, new { Id = id });

        return product ?? new Product();
    }

    public async Task<int> AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        const string sql = """
        INSERT INTO Products (Name, Description, Price, Category, ImageUrl, Tags, Metadata)
        OUTPUT INSERTED.Id
        VALUES (@Name, @Description, @Price, @Category, @ImageUrl, @Tags, @Metadata);
        """;

        var command = new CommandDefinition(sql, product, cancellationToken: cancellationToken);

        return await _connection.QuerySingleAsync<int>(command);
    }

    public async Task UpdateAsync(Product product)
    {
        const string sql = """
        UPDATE Products 
        SET Name = @Name, 
            Description = @Description, 
            Price = @Price, 
            Category = @Category,
            ImageUrl = @ImageUrl,
            Tags = @Tags,
            Metadata = @Metadata
        WHERE Id = @Id
        """;

        await _connection.ExecuteAsync(sql, product);
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "DELETE FROM Products WHERE Id = @Id";
        await _connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<List<Product>> SearchAsync(string query)
    {
        const string sql = """
            SELECT Id, Name, Description, Price, Category
            FROM Products
            WHERE Name LIKE '%' + @Query + '%'
               OR Description LIKE '%' + @Query + '%'
            """;
        return (await _connection.QueryAsync<Product>(sql, new { Query = query })).ToList();
    }

    public async Task<List<Product>> GetByCategoryAsync(string category)
    {
        const string sql = """
            SELECT Name, Description, Price, Category, ImageUrl, Tags, Metadata
            FROM Products
            WHERE Category = @Category
            """;
        return (await _connection.QueryAsync<Product>(sql, new { Category = category })).ToList();
    }

    public async Task<List<Product>> GetAllAsync()
    {
        const string sql = """
            SELECT Name, Description, Price, Category, ImageUrl, Tags, Metadata
            FROM Products
            """;
        return (await _connection.QueryAsync<Product>(sql)).ToList();
    }
}