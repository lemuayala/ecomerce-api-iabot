using MediatR;

namespace EcomerceAI.Api.Features.Products.Application.Commands;

public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    string Category) : IRequest<int>;