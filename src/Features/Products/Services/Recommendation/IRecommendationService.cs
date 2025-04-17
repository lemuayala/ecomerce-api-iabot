public interface IRecommendationService
{
    Task<List<ProductRecommendation>> GetPersonalizedRecommendations(int userId);
    Task<List<ProductRecommendation>> GetSimilarProducts(int productId);
}