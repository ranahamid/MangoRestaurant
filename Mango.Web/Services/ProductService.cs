using Mango.Web.Models;
using Mango.Web.Services.IServices;

namespace Mango.Web.Services
{
    public class ProductService:IProductService
    {
        public Task<T> GetAllProductAsync<T>()
        {
            throw new NotImplementedException();
        }

        public Task<T> GetAllProductByIdAsync<T>(int id)
        {
            throw new NotImplementedException();
        }

        public Task<T> CreateProductAsync<T>(ProductDto productDto)
        {
            throw new NotImplementedException();
        }

        public Task<T> UpdateProductAsync<T>(ProductDto productDto)
        {
            throw new NotImplementedException();
        }

        public Task<T> DeleteProductAsync<T>(int id)
        {
            throw new NotImplementedException();
        }
    }
}
