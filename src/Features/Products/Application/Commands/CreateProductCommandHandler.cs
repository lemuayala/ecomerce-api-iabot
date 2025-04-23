using EcomerceAI.Api.Features.Products.Domain.Models;
using EcomerceAI.Application.Features.Products.Commands;
using MediatR;

namespace EcomerceAI.Application.Features.Products.Handlers;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Product>
{
    private readonly IAzureStorageService _storageService;
    private readonly IProductRepository _productRepository;

    public CreateProductCommandHandler(IAzureStorageService storageService, IProductRepository productRepository)
    {
        _storageService = storageService;
        _productRepository = productRepository;
    }

    public async Task<Product> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        string? imageUrl = null;

        // Intentar subir la imagen PRIMERO
        if (request.ImageFile != null)
        {
            try
            {
                // Pasamos cancellationToken por si la operación se cancela
                imageUrl = await _storageService.UploadImageAsync(request.ImageFile, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error al subir imagen: {ex.Message}", ex);
            }
        }

        // Crear el objeto Product
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Category = request.Category,
            ImageUrl = imageUrl,
            Tags = request.Tags ?? new List<string>(),
            Metadata = request.Metadata
        };

        // Intentar guardar en la Base de Datos
        try
        {
            await _productRepository.AddAsync(product, cancellationToken);
            return product;
        }
        catch (Exception dbEx)
        {
            // ¡FALLÓ LA INSERCIÓN EN LA BD!
            if (imageUrl != null)
            {
                try
                {
                    await _storageService.DeleteImageAsync(imageUrl, cancellationToken);
                }
                catch (Exception storageDeleteEx)
                {
                    throw new ApplicationException($"Error al guardar producto en BD: {dbEx.Message}. ADEMÁS, falló la limpieza de la imagen '{imageUrl}': {storageDeleteEx.Message}", dbEx);
                }
            }

            throw new ApplicationException($"Error al guardar producto en BD: {dbEx.Message}", dbEx);
        }
    }
}
