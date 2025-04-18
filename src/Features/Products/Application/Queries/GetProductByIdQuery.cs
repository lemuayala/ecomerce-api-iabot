using MediatR;

namespace EcomerceAI.Api.Features.Products.Application.Queries
{
    public record GetProductByIdQuery(int Id) : IRequest<Product>;
}