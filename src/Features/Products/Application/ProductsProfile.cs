
using AutoMapper;
using EcomerceAI.Api.Features.Products.Application.Commands;

namespace EcomerceAI.Api.Features.Products.Application;

public class ProductsProfile : Profile
{
    public ProductsProfile()
    {
        CreateMap<CreateProductCommand, Product>();
    }
}