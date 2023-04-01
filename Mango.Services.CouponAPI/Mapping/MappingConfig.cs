using AutoMapper;


namespace Mango.Services.CouponAPI.Mapping
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(x =>
            {
               // x.CreateMap<ProductDto, Product>().ReverseMap(); 
            });
            return mappingConfig;
        }
    }
}
