using AutoMapper;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.dtos;

namespace Mango.Services.ShoppingCartAPI.Mapping
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(x =>
            {
                x.CreateMap<ProductDto, Product>().ReverseMap();
                x.CreateMap<CartDetailsDto, CartDetails>().ReverseMap();
                x.CreateMap<CartHeaderDto, CartHeader>().ReverseMap();
                x.CreateMap<CartDto, Cart>().ReverseMap();
            });
            return mappingConfig;
        }
    }
}
