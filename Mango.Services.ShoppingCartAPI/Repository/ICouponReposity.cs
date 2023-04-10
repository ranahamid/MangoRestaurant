using Mango.Services.ShoppingCartAPI.Models.Dto;

namespace Mango.Services.ShoppingCartAPI.Repository
{
    public interface ICouponReposity
    {
        Task<CouponDto> GetCoupon(string couponName);

    }
}
