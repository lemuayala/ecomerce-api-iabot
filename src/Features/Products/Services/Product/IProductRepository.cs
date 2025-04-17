public interface IProductRepository
{
    Task<Product> GetByIdAsync(int id);
    Task<List<Product>> SearchAsync(string query);
    Task<List<Product>> GetByCategoryAsync(string category);
}