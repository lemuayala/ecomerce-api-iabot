using EcomerceAI.Api.Features.Products.Domain.Models;

public class ProductRecommendation : Product
{
    public float RelevanceScore { get; set; }
    public required string RecommendationReason { get; set; }
}