// src/Features/Products/Application/Commands/CreateProductCommandHandler.cs
using MediatR;

namespace EcomerceAI.Api.Features.Products.Application.Commands;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
{
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Category = request.Category
        };

        return await _repository.AddAsync(product);
    }
}