using Mango.Web.Extensions;
using Mango.Web.Models;
using Mango.Web.Services.IServices;

namespace Mango.Web.Services
{
    public class CartService: BaseService, ICartService
    {
        private readonly IHttpClientFactory _clientFactory;
        public CartService(IHttpClientFactory client) : base(client)
        {
            _clientFactory = client;
        }

        public async Task<T> AddToCartAsync<T>(CartDto cartDto, string token)
        {
            return await this.SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.POST,
                Data= cartDto,
                Url = SD.ShoppingCartAPIBase + "api/cartAPI/AddCart",
                AccessToken = token
            });
        }

        public async Task<T> GetCartByUserIdAsync<T>(string userId, string token = null)
        {
            return await this.SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.GET, 
                Url = SD.ShoppingCartAPIBase + "api/cartAPI/GetCart/"+ userId,
                AccessToken = token
            });
        }

        public async Task<T> RemoveFromCartAsync<T>(int cartId, string token = null)
        {
            return await this.SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = cartId,
                Url = SD.ShoppingCartAPIBase + "api/cartAPI/RemoveCart",
                AccessToken = token
            });
        }

        public async Task<T> UpdateCartAsync<T>(CartDto cartDto, string token)
        {
            return await this.SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = cartDto,
                Url = SD.ShoppingCartAPIBase + "api/cartAPI/UpdateCart",
                AccessToken = token
            });
        }

        public async Task<T> ApplyCoupon<T>(CartDto cartDto, string token)
        {
            return await this.SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = cartDto,
                Url = SD.ShoppingCartAPIBase + "api/cartAPI/ApplyCoupon",
                AccessToken = token
            });
        }
        public async Task<T> RemoveCoupon<T>(string userId, string token)
        {
            return await this.SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = userId,
                Url = SD.ShoppingCartAPIBase + "api/cartAPI/RemoveCoupon",
                AccessToken = token
            });
        }

        public async Task<T> Checkout<T>(CartHeaderDto cartHeader, string token = null)
        {
            cartHeader= cartHeader.ConvertNullPropertiesToDefault();
            cartHeader.CartDetails= new List<CartDetailsDto>();
            return await this.SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = cartHeader,
                Url = SD.ShoppingCartAPIBase + "api/cartAPI/checkout",
                AccessToken = token
            });
        }
        
    }
}
