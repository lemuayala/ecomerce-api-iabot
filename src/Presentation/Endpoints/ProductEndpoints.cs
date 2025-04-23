using EcomerceAI.Api.Features.Products.Domain.Models;
using EcomerceAI.Application.Features.Products.Commands;
using MediatR;
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
        productGroup.MapPost("/", async Task<IResult> (
                    [FromForm] CreateProductRequest request,
                    IMediator mediator,
                    ILogger<Product> logger) =>
                    {
                        try
                        {
                            // Crear el comando con la request recibida
                            var command = new CreateProductCommand(request);
                            // Enviar el comando al handler a través de MediatR
                            var createdProduct = await mediator.Send(command);

                            // Si MediatR no lanzó excepción, todo fue bien
                            logger.LogInformation("Producto creado exitosamente con ID {ProductId}", createdProduct.Id); // Example log
                            return Results.Ok(createdProduct);
                        }
                        catch (ApplicationException appEx)
                        {
                            logger.LogError(appEx, "Error de aplicación al crear producto: {ErrorMessage}", appEx.Message);
                            return Results.UnprocessableEntity(appEx.Message);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Error inesperado durante la creación del producto.");
                            return Results.BadRequest($"Ocurrió un error inesperado: {ex.Message}");
                        }
                    })
                     .Accepts<CreateProductRequest>("multipart/form-data")
                     .Produces<Product>(StatusCodes.Status200OK)
                     .Produces<string>(StatusCodes.Status400BadRequest)
                     .Produces<string>(StatusCodes.Status422UnprocessableEntity)
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