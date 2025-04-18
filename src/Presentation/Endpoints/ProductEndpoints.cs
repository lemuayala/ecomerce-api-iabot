using MediatR;
using EcomerceAI.Api.Features.Products.Application.Queries;
using EcomerceAI.Api.Features.Products.Application.Commands;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        app.MapGet("/{id}",
            async (int id, IMediator mediator) =>
            {
                var product = await mediator.Send(new GetProductByIdQuery(id));
                return product is not null ? Results.Ok(product) : Results.NotFound();
            });

        app.MapPost("/",
            async (CreateProductCommand command, IMediator mediator) =>
            {
                var productId = await mediator.Send(command);
                return Results.Created($"/products/{productId}", null);
            });
    }
}