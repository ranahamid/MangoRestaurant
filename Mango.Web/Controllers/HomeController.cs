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

        public HomeController(ILogger<HomeController> logger, IProductService productService)
        {
            _logger = logger;
            _productService = productService;
        } 
        public async Task <IActionResult> Index()
        {
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
            if (response.IsSuccess)
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