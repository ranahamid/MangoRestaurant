using Mango.Services.CouponAPI.Models.dtos;
using Mango.Services.CouponAPI.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.CouponAPI.Controllers
{
    [Route("api/CouponAPI")]
    //[Route("api/[controller]")]
    [ApiController]
    public class CouponAPIController : ControllerBase
    {
        protected ResponseDto _response;
        private ICouponRepository _couponRepository;
        public CouponAPIController(ICouponRepository couponRepository)
        {
            _couponRepository = couponRepository;
            this._response = new ResponseDto();
        }

        //[Authorize]
        [HttpGet]
        [Route("GetDiscountForCode/{code}")]
        public async Task<object> GetDiscountForCode(string code)
        {
            try
            {
                var coupon = await _couponRepository.GetCouponByCode(code);
                _response.Result = coupon;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessage = new List<string>() { ex.ToString() };
            }

            return _response;
        }
    }
}
