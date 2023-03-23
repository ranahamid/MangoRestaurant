using Mango.Web.Models;
using Mango.Web.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;

namespace Mango.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }
        public async Task<IActionResult> ProductIndex()
        {
            var list = new List<ProductDto>();
            var response = await _productService.GetAllProductAsync<ResponseDto>();
            if (response.IsSuccess)
            {
                var result = Convert.ToString(response.Result);
                if (result != null)
                    list = JsonConvert.DeserializeObject<List<ProductDto>>(result);
            }
            return View(list);
        }
        public async Task<IActionResult> ProductCreate()
        { 
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProductCreate(ProductDto model)
        {
            if (ModelState.IsValid)
            {
                var response = await _productService.CreateProductAsync<ResponseDto>(model);
                if (response.IsSuccess)
                {
                   return RedirectToAction(nameof(ProductIndex));
                }
            }
            return View(model);
        }

        public async Task<IActionResult> ProductEdit(int productId)
        {
            var response = await _productService.GetAllProductByIdAsync<ResponseDto>(productId);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProductEdit(ProductDto model)
        {
            if (ModelState.IsValid)
            {
                var response = await _productService.UpdateProductAsync<ResponseDto>(model);
                if (response.IsSuccess)
                {
                    return RedirectToAction(nameof(ProductIndex));
                }
            }
            return View(model);
        }


        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> ProductDelete(int productId)
        {
            var response = await _productService.GetAllProductByIdAsync<ResponseDto>(productId);
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

        //[Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProductDelete(ProductDto model)
        {
            if (ModelState.IsValid)
            {
                var response = await _productService.DeleteProductAsync<ResponseDto>(model.ProductId);
                if (response.IsSuccess)
                {
                    return RedirectToAction(nameof(ProductIndex));
                }
            }
            return View(model);
        }





    }
}
