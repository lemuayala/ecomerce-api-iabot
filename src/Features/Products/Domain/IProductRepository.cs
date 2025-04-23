using EcomerceAI.Api.Features.Products.Domain.Models;

public interface IProductRepository
{
    Task<Product> GetByIdAsync(int id);
    Task<int> AddAsync(Product product, CancellationToken cancellationToken = default);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
    Task<List<Product>> GetAllAsync();
    Task<List<Product>> SearchAsync(string query);
    Task<List<Product>> GetByCategoryAsync(string category);
}