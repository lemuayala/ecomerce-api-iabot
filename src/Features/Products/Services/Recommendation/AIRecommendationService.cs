using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

public class AIRecommendationService : IRecommendationService
{
    private readonly Kernel _kernel;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<AIRecommendationService> _logger;

    public AIRecommendationService(
        Kernel kernel,
        IProductRepository productRepository,
        ILogger<AIRecommendationService> logger)
    {
        _kernel = kernel;
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<List<ProductRecommendation>> GetPersonalizedRecommendations(int userId)
    {
        try
        {
            // 1. Obtener historial del usuario (simulado)
            var userHistory = new
            {
                ViewedProducts = new[] { 101, 205, 308 },
                PurchasedProducts = new[] { 205 },
                SearchTerms = new[] { "electronics", "gadgets" }
            };

            // 2. Crear prompt contextualizado
            var prompt = $"""
                Eres un asistente de recomendaciones para un e-commerce.
                Historial del usuario {userId}:
                - Productos vistos: {string.Join(", ", userHistory.ViewedProducts)}
                - Productos comprados: {string.Join(", ", userHistory.PurchasedProducts)}
                - Términos buscados: {string.Join(", ", userHistory.SearchTerms)}
                
                Genera 5 recomendaciones personalizadas con razones detalladas.
                Devuelve SOLO los IDs de productos en formato JSON array.
                Ejemplo: [101, 205, 308]
                """;

            // 3. Configurar opciones de ejecución
            var executionSettings = new OpenAIPromptExecutionSettings
            {
                Temperature = 0.7,
                MaxTokens = 500,
                ResponseFormat = "json_object"
            };

            // 4. Ejecutar el modelo de IA
            var result = await _kernel.InvokePromptAsync<string>(
                prompt,
                new(executionSettings));

            _logger.LogInformation("Respuesta de IA recibida: {Response}", result);

            // 5. Parsear y procesar resultados
            return await ProcessAIResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener recomendaciones");
            return new List<ProductRecommendation>(); // Devuelve lista vacía en caso de error
        }
    }

    public Task<List<ProductRecommendation>> GetSimilarProducts(int productId)
    {
        throw new NotImplementedException();
    }

    private async Task<List<ProductRecommendation>> ProcessAIResponse(string aiResponse)
    {
        try
        {
            // Parsear la respuesta JSON
            var recommendedIds = JsonSerializer.Deserialize<List<int>>(aiResponse)
                ?? new List<int> { 101, 205, 308 }; // Fallback

            var products = new List<ProductRecommendation>();
            foreach (var id in recommendedIds)
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product != null)
                {
                    products.Add(new ProductRecommendation
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Description = product.Description,
                        Price = product.Price,
                        Category = product.Category,
                        RelevanceScore = Random.Shared.NextSingle() * 0.5f + 0.5f,
                        RecommendationReason = "Basado en tu historial de navegación"
                    });
                }
            }

            return products.OrderByDescending(p => p.RelevanceScore).ToList();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error al parsear respuesta de IA");
            return new List<ProductRecommendation>();
        }
    }
}