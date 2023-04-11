using AutoMapper;
using Mango.Services.Email.Messages;
using Mango.Services.Email.Models;

namespace Mango.Services.Email.Mapping
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(x =>
            {
              //  x.CreateMap<CheckoutHeaderDto, OrderHeader>().ReverseMap(); 
            });
            return mappingConfig;
        }
    }
}
