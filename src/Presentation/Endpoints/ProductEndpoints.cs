using MediatR;
using EcomerceAI.Api.Features.Products.Application.Commands;
using EcomerceAI.Api.Features.Products.Application.Queries;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        var productGroup = app.MapGroup("/api/products");

        // Endpoint para obtener producto por ID usando CQRS
        productGroup.MapGet("/{id}", async (int id, IMediator mediator) =>
            await mediator.Send(new GetProductByIdQuery(id)) is { } product
                ? Results.Ok(product)
                : Results.NotFound());

        // Endpoint para crear producto
        productGroup.MapPost("/", async (CreateProductCommand command, IMediator mediator) =>
        {
            var productId = await mediator.Send(command);
            return Results.Created($"/api/products/{productId}", null);
        });

        // Endpoint para recomendaciones personalizadas
        productGroup.MapGet("/recommendations/{userId}",
            async (int userId, IRecommendationService service) =>
                Results.Ok(await service.GetPersonalizedRecommendations(userId)));

        // Endpoint de búsqueda
        productGroup.MapGet("/search", async (string query, IProductRepository repo) =>
        {
            if (string.IsNullOrWhiteSpace(query))
                return Results.BadRequest("Query parameter is required");

            return Results.Ok(await repo.SearchAsync(query));
        });
    }
}