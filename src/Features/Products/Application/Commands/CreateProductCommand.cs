
using EcomerceAI.Api.Features.Products.Domain.Models;
using MediatR;

namespace EcomerceAI.Application.Features.Products.Commands;

public record CreateProductCommand(CreateProductRequest Request) : IRequest<Product>;
