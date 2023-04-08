using Mango.MessageBus;
using Mango.Services.ShoppingCartAPI.Messages;
using Mango.Services.ShoppingCartAPI.Models.dtos;
using Mango.Services.ShoppingCartAPI.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.ShoppingCartAPI.Controllers
{
    [Route("api/CartAPI")]
    [ApiController]
    public class CartAPIController : ControllerBase
    {
        protected ResponseDto _response;
        private ICartReporsitory _cartRepository;
        private readonly IMessageBus _messageBus;
        public CartAPIController(ICartReporsitory cartReporsitory, IMessageBus messageBus)
        {
            _cartRepository = cartReporsitory;
            _messageBus = messageBus;
            this._response = new ResponseDto();
        }
        //[Authorize]
        [HttpGet]
        [Route("GetCart/{userId}")] 
        public async Task<object> GetCart(string userId)
        {
            try
            {
                var cartDto = await _cartRepository.GetCartByUserId(userId);
                _response.Result = cartDto;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }

            return _response;
        }

        [HttpPost]
        [Route("AddCart")]
        public async Task<object> AddCart(CartDto cartDto)
        {
            try
            {
                var data = await _cartRepository.CreateUpdateCart(cartDto);
                _response.Result = data;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }

            return _response;
        }

        [HttpPost]
        [Route("UpdateCart")]
        public async Task<object> UpdateCart(CartDto cartDto)
        {
            try
            {
                var cartDt = await _cartRepository.CreateUpdateCart(cartDto);
                _response.Result = cartDt;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }

            return _response;
        }

        [HttpPost("RemoveCart")] 
        public async Task<object> RemoveCart([FromBody] int cartId)
        {
            try
            {
                var cartDt = await _cartRepository.RemoveFromCart(cartId);
                _response.Result = cartDt;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }

            return _response;
        }

        [HttpPost("Checkout")]
        public async Task<object> Checkout(CheckoutHeaderDto checkoutHeader)
        {
            try
            {
                CartDto cartDto = await _cartRepository.GetCartByUserId(checkoutHeader.UserId);
                if (cartDto == null)
                {
                    return BadRequest();
                }
                checkoutHeader.CartDetails = cartDto.CartDetails;
                //logic to add message to process order.
                await _messageBus.PublishMessage(checkoutHeader, "checkoutmessagetopic");
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }
        //[HttpPost]
        //[Route("Checkout")]
        //public async Task<object> Checkout(  CheckoutHeaderDto cardHeaderDto)
        //{

        //    try
        //    {
        //        var cartDto = await _cartReporsitory.GetCartByUserId(cardHeaderDto.UserId);
        //        if(cartDto == null)
        //        {
        //            return BadRequest();
        //        }
        //       // cardHeaderDto.CartDetails = cartDto.CartDetails;
        //        //logic to add message to process order 

        //    }
        //    catch (Exception ex)
        //    {
        //        _response.IsSuccess = false;
        //        _response.ErrorMessage = new List<string>() { ex.ToString() };
        //    }

        //    return _response;
        //}


        [HttpPost("ClearCart")] 
        public async Task<object> ClearCart([FromBody] string userId)
        {
            try
            {
                var cartDt = await _cartRepository.ClearCart(userId);
                _response.Result = cartDt;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }

            return _response;
        }

        #region coupon
        [HttpPost("ApplyCoupon")] 
        public async Task<object> ApplyCoupon([FromBody] CartDto cartDto)
        {
            try
            {
                var cartDt = await _cartRepository.ApplyCoupon(cartDto.CartHeader.UserId, cartDto.CartHeader.CouponCode);
                _response.Result = cartDt;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }

            return _response;
        }
        [HttpPost("RemoveCoupon")]
        public async Task<object> RemoveCoupon([FromBody] string userId)
        {
            try
            {
                var cartDt = await _cartRepository.RemoveCoupon(userId);
                _response.Result = cartDt;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }

            return _response;
        }

        #endregion


    }
}
