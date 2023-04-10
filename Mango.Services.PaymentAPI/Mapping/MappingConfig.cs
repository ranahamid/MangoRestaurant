using AutoMapper; 

namespace Mango.Services.PaymentAPI.Mapping
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(x =>
            {
                
               // x.CreateMap<CartDetailsDto,OrderDetails>().ReverseMap();
            });
            return mappingConfig;
        }
    }
}
