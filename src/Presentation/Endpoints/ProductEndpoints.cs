
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
        productGroup.MapPost("/", async (Product product, IProductRepository repo) =>
        {
            var productId = await repo.AddAsync(product);
            return Results.Created($"/api/products/{productId}", null);
        });

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