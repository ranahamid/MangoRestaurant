using AutoMapper;
using Mango.Services.OrderAPI.Messages;
using Mango.Services.OrderAPI.Models;

namespace Mango.Services.OrderAPI.Mapping
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(x =>
            {
                x.CreateMap<CheckoutHeaderDto, OrderHeader>().ReverseMap();
                x.CreateMap<CartDetailsDto,OrderDetails>().ReverseMap();
            });
            return mappingConfig;
        }
    }
}
