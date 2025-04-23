using EcomerceAI.Api.Features.Products.Domain.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        var productGroup = app.MapGroup("/api/products").WithTags("Products");

        // Obtener producto por ID
        productGroup.MapGet("/{id}", async (int id, IProductRepository repo) =>
            await repo.GetByIdAsync(id) is { } product
                ? Results.Ok(product)
                : Results.NotFound());

        // Crear producto
        productGroup.MapPost("/", async Task<Results<Ok<Product>, BadRequest<string>>> (
            [FromForm] CreateProductRequest request,
            IAzureStorageService storageService,
            IProductRepository repo) =>
        {
            // Subir imagen si existe
            string imageUrl = null;
            if (request.ImageFile != null)
            {
                try
                {
                    imageUrl = await storageService.UploadImageAsync(request.ImageFile);
                }
                catch (Exception ex)
                {
                    return TypedResults.BadRequest($"Error al subir imagen: {ex.Message}");
                }
            }

            // Crear producto
            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Category = request.Category,
                ImageUrl = imageUrl,
                Tags = request.Tags ?? new List<string>(),
                Metadata = request.Metadata ?? new Dictionary<string, string>()
            };

            await repo.AddAsync(product);

            return TypedResults.Ok(product);
        })
        .Accepts<CreateProductRequest>("multipart/form-data")
        .Produces<Product>(StatusCodes.Status200OK)
        .Produces<string>(StatusCodes.Status400BadRequest)
        .DisableAntiforgery();

        // Actualizar producto
        productGroup.MapPut("/{id}", async (int id, Product product, IProductRepository repo) =>
        {
            if (id != product.Id) return Results.BadRequest();
            await repo.UpdateAsync(product);
            return Results.Ok();
        });

        // Eliminar producto
        productGroup.MapDelete("/{id}", async (int id, IProductRepository repo) =>
        {
            await repo.DeleteAsync(id);
            return Results.NoContent();
        });

        // Obtener todos
        productGroup.MapGet("/", async (IProductRepository repo) =>
            Results.Ok(await repo.GetAllAsync()));

        // Búsqueda
        productGroup.MapGet("/search", async (string query, IProductRepository repo) =>
        {
            if (string.IsNullOrWhiteSpace(query))
                return Results.BadRequest("Query parameter is required");

            return Results.Ok(await repo.SearchAsync(query));
        });

        // Recomendaciones (Servicio especializado con IA)
        productGroup.MapGet("/recommendations/{userId}",
            async (int userId, IRecommendationService service) =>
                Results.Ok(await service.GetPersonalizedRecommendations(userId)));
    }
}