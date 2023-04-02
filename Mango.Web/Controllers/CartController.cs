using Mango.Web.Models;
using Mango.Web.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;


namespace Mango.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICartService _cartService;
        private readonly ICouponService _couponService;
     public CartController(IProductService productService, ICartService cartService, ICouponService couponService)
        {
            _productService = productService;
            _cartService = cartService;
            _couponService = couponService;
        }
        public async Task<IActionResult> CartIndex()
        {
            var model= await LoadCartDtoBasedOnLoggedInUser();
            return View(model);
        }
        public async Task<IActionResult> Remove(int cartDetailsId)
        {
            var userId = User.Claims.Where(x => x.Type == "sub")?.FirstOrDefault()?.Value;
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var response = await _cartService.RemoveFromCartAsync<ResponseDto>(cartDetailsId, accessToken);
            
            if (response != null && response.IsSuccess)
            {
                return RedirectToAction(nameof(CartIndex));
            }
            return View();

        }
        private async Task<CartDto> LoadCartDtoBasedOnLoggedInUser()
        {
            var userId = User.Claims.Where(x => x.Type == "sub")?.FirstOrDefault()?.Value;
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var response = await _cartService.GetCartByUserIdAsync<ResponseDto>(userId, accessToken);
            var model = new CartDto();
            if (response != null && response.IsSuccess)
            {
                var result = Convert.ToString(response.Result);
                if (result != null)
                {
                      model = JsonConvert.DeserializeObject<CartDto>(result); 
                }
            }
            if(model.CartHeader!=null)
            {
                if (!string.IsNullOrEmpty(model.CartHeader.CouponCode))
                {
                    var coupon = await _couponService.GetCoupon<ResponseDto>(model.CartHeader.CouponCode, accessToken);
                    var result = Convert.ToString(coupon.Result);
                    if (result != null)
                    {
                        var couponObj = JsonConvert.DeserializeObject<CouponDto>(result);
                        model.CartHeader.DiscountTotal = couponObj.DiscountAmount;
                    }
                }
                double total = 0;
                foreach(var detail in model.CartDetails)
                {
                    total += detail.Product.Price * detail.Count;
                }
                model.CartHeader.OrderTotal = total;
                if(total >= model.CartHeader.DiscountTotal)
                {
                    model.CartHeader.OrderTotal -= model.CartHeader.DiscountTotal;
                }
            }
            return model;
        }

        #region coupon
        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(CartDto cartDto)
        {
            var userId = User.Claims.Where(x => x.Type == "sub")?.FirstOrDefault()?.Value;
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var response = await _cartService.ApplyCoupon<ResponseDto>(cartDto, accessToken);

            if (response != null && response.IsSuccess)
            {
                return RedirectToAction(nameof(CartIndex));
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> RemoveCoupon(CartDto cartDto)
        { 
            var userId = User.Claims.Where(x => x.Type == "sub")?.FirstOrDefault()?.Value;
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var response = await _cartService.RemoveCoupon<ResponseDto>(cartDto.CartHeader.UserId, accessToken);

            if (response != null && response.IsSuccess)
            {
                return RedirectToAction(nameof(CartIndex));
            }
            return View();
        }

        #endregion 
    }
}
