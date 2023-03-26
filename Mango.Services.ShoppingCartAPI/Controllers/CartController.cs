﻿using Mango.Services.ShoppingCartAPI.Models.dtos;
using Mango.Services.ShoppingCartAPI.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.ShoppingCartAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        protected ResponseDto _response;
        private ICartReporsitory _cartReporsitory;

        public CartController(ICartReporsitory cartReporsitory)
        {
            _cartReporsitory = cartReporsitory;
            this._response = new ResponseDto();
        }
        //[Authorize]
        [HttpGet]
        [Route("{userId}")]
        // [HttpGet("GetCart/{userId}")]
        public async Task<object> GetCart(string userId)
        {
            try
            {
                var cartDto = await _cartReporsitory.GetCartByUserId(userId);
                _response.Result = cartDto;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessage = new List<string>() { ex.ToString() };
            }

            return _response;
        }

        [HttpPost("AddCart")]
     
        public async Task<object> AddCart(CartDto cartDto)
        {
            try
            {
                var cartDt = await _cartReporsitory.CreateUpdateCart(cartDto);
                _response.Result = cartDt;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessage = new List<string>() { ex.ToString() };
            }

            return _response;
        }

        [HttpPost("UpdateCart")]
        public async Task<object> UpdateCart(CartDto cartDto)
        {
            try
            {
                var cartDt = await _cartReporsitory.CreateUpdateCart(cartDto);
                _response.Result = cartDt;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessage = new List<string>() { ex.ToString() };
            }

            return _response;
        }

        [HttpPost("RemoveCart")]
        [Route("{cartId}")]
        public async Task<object> RemoveCart([FromBody] int cartId)
        {
            try
            {
                var cartDt = await _cartReporsitory.RemoveFromCart(cartId);
                _response.Result = cartDt;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessage = new List<string>() { ex.ToString() };
            }

            return _response;
        }

        [HttpPost("ClearCart")]
        [Route("{userId}")]
        public async Task<object> ClearCart([FromBody] string userId)
        {
            try
            {
                var cartDt = await _cartReporsitory.ClearCart(userId);
                _response.Result = cartDt;
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
