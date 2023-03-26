using Mango.Web.Models;
using Mango.Web.Services.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Mango.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductService _productService;
        private readonly ICartService _cartService;
        public HomeController(ILogger<HomeController> logger, IProductService productService, ICartService cartService)
        {
            _logger = logger;
            _productService = productService;
            _cartService = cartService;
        } 
        public async Task <IActionResult> Index()
        {
            //var userId = User.Claims.Where(x => x.Type == "sub")?.FirstOrDefault()?.Value;

            var accessToken = "";// await HttpContext.GetTokenAsync("access_token");
            var list = new List<ProductDto>();
            var response = await _productService.GetAllProductAsync<ResponseDto>(accessToken);
            if (response != null && response.IsSuccess)
            {
                var result = Convert.ToString(response.Result);
                if (result != null)
                    list = JsonConvert.DeserializeObject<List<ProductDto>>(result);
            }
            return View(list);
        }

        [Authorize]
        public async Task<IActionResult> ProductDetails(int productId)
        {
            var accessToken =   await HttpContext.GetTokenAsync("access_token");
            var response = await _productService.GetAllProductByIdAsync<ResponseDto>(productId, accessToken);
            if (response!=null && response.IsSuccess)
            {
                var result = Convert.ToString(response.Result);
                if (result != null)
                {
                    var model = JsonConvert.DeserializeObject<ProductDto>(result);
                    return View(model);
                }
            }
            return NotFound();
        }

        [HttpPost]
        [ActionName("ProductDetails")]
        [Authorize]
        public async Task<IActionResult> DetailsPost(ProductDto productDto)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            //var userId = User.Claims.Where(x => x.Type .Contains("nameidentifier"))?.FirstOrDefault()?.Value;
            var userId = User.Claims.Where(x => x.Type=="sub")?.FirstOrDefault()?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                userId = User.Claims.Where(x => x.Type.Contains("nameidentifier"))?.FirstOrDefault()?.Value;
            }
            CartDto cartDto = new CartDto
            {
                CartHeader = new CartHeaderDto
                {
                    UserId = userId,
                },  
            };
            CartDetailsDto cartDetails = new CartDetailsDto
            {
                Count = productDto.Count,
                ProductId = productDto.ProductId,
                CartHeader= new CartHeaderDto
                {
                    UserId = userId,
                } 
            };
            var response = await _productService.GetAllProductByIdAsync<ResponseDto>(productDto.ProductId, accessToken);
            if(response!=null && response.IsSuccess)
            {
                cartDetails.Product = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(response.Result));
            }
            List<CartDetailsDto> cartDetailsList = new List<CartDetailsDto>();
            cartDetailsList.Add(cartDetails);

            cartDto.CartDetails = cartDetailsList;
            var addToCartResponse = await _cartService.AddToCartAsync<ResponseDto>(cartDto, accessToken);
            if (addToCartResponse != null && addToCartResponse.IsSuccess)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(productDto);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [Authorize]
		public async Task<IActionResult> Login()
		 {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
			return RedirectToAction(nameof(Index)); 
		}
		public IActionResult Logout()
		{
			return SignOut("Cookies", "oidc");
		}
	}
}