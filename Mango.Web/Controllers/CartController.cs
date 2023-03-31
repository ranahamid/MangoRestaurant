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
        public CartController(IProductService productService,ICartService cartService)
        {
            _productService = productService;
            _cartService = cartService;
        }
        public async Task<IActionResult> CartIndex()
        {
            var model= await LoadCartDtoBasedOnLoggedInUser();
            return View(model);
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
                double total = 0;
                foreach(var detail in model.CartDetails)
                {
                    total += detail.Product.Price * detail.Count;
                }
                model.CartHeader.OrderTotal = total;
            }
            return model;
        }
    }
}
