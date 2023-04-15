using Mango.Web.Models;
using Mango.Web.Services.IServices;

namespace Mango.Web.Services
{
    public class ProductService: BaseService, IProductService
    {
        private readonly IHttpClientFactory _clientFactory;
        public ProductService(IHttpClientFactory client) : base(client)
        {
            _clientFactory = client;
        }
        public  async Task<T> GetAllProductAsync<T>(string token)
        {
            return await this.SendAsync<T>(new ApiRequest()
            { 
                ApiType = SD.ApiType.GET,
                Url = SD.ProductAPIBase + "api/ProductAPI" ,
                AccessToken = token
            });
        }

        public async Task<T> GetAllProductByIdAsync<T>(int id, string token)
        {
            return await this.SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.GET, 
                Url = SD.ProductAPIBase + "api/ProductAPI" + id,
                AccessToken = token
            });
        }

        public async Task<T> CreateProductAsync<T>(ProductDto productDto, string token)
        {
            return await this.SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = productDto,
                Url = SD.ProductAPIBase+ "api/ProductAPI",
                AccessToken = token
            });

        }

        public async Task<T> UpdateProductAsync<T>(ProductDto productDto, string token)
        {
            return await this.SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.PUT,
                Data = productDto,
                Url = SD.ProductAPIBase + "api/ProductAPI",
                AccessToken = token
            });
        }

        public async  Task<T> DeleteProductAsync<T>(int id, string token) 
        {
            return await this.SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.DELETE,
                
                Url = SD.ProductAPIBase + "api/ProductAPI"+id,
                AccessToken = token
            });
        }

        
    }
}
