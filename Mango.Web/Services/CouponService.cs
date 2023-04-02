using Mango.Web.Models;
using Mango.Web.Services.IServices;

namespace Mango.Web.Services
{
    public class CouponService : BaseService, ICouponService
    {
        private readonly IHttpClientFactory _clientFactory;
        public CouponService(IHttpClientFactory client) : base(client)
        {
            _clientFactory = client;
        }
        public async Task<T> GetCoupon<T>(string couponCode, string token)
        {
            return await this.SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.GET,
                Data= couponCode,
                Url = SD.CouponAPIBase + "api/CouponAPI/GetDiscountForCode/" + couponCode,
                AccessToken = token
            });
        }
    }
}
