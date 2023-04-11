using Mango.MessageBus;
using Mango.Services.ShoppingCartAPI.Messages;
using Mango.Services.ShoppingCartAPI.Models.dtos;
using Mango.Services.ShoppingCartAPI.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Mango.Services.ShoppingCartAPI.Controllers
{
    [Route("api/CartAPI")]
    [ApiController]
    public class CartAPIController : ControllerBase
    {
        protected ResponseDto _response;
        private ICartReporsitory _cartRepository;
        private readonly IMessageBus _messageBus;

        private readonly string serviceBusConnectionString;
        private readonly string subscriptionName;
        private readonly string checkoutMessageTopic;
        private readonly IConfiguration _configuration;

        private readonly ICouponReposity _couponReposity;
        public CartAPIController(ICartReporsitory cartReporsitory, IMessageBus messageBus, IConfiguration configuration, ICouponReposity couponReposity)
        {
            _cartRepository = cartReporsitory;
            _messageBus = messageBus;
            this._response = new ResponseDto();

            _configuration = configuration;
            _couponReposity = couponReposity;

            serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");
            subscriptionName = _configuration.GetValue<string>("subscriptionNameCheckout");
            checkoutMessageTopic = _configuration.GetValue<string>("CheckoutMessageTopic");
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
                if (!string.IsNullOrEmpty(checkoutHeader.CouponCode))
                {
                    var coupon= await _couponReposity.GetCoupon(checkoutHeader.CouponCode);
                    if (checkoutHeader.DiscountTotal != coupon.DiscountAmount)
                    {
                        //checkoutHeader.OrderTotal = checkoutHeader.OrderTotal + checkoutHeader.DiscountTotal - coupon.DiscountAmount;
                        //checkoutHeader.DiscountTotal = coupon.DiscountAmount;
                        _response.IsSuccess = false;
                        _response.ErrorMessages = new List<string>() {"Coupon price has changed, please confirm." };
                        _response.DisplayMessage = "Coupon price has changed, please confirm.";
                        return _response;
                    }
                }
                checkoutHeader.CartDetails = cartDto.CartDetails;
                //logic to add message to process order.
                await _messageBus.PublishMessage(checkoutHeader, checkoutMessageTopic/* "checkoutmessagetopic"*/);
                await _cartRepository.ClearCart(checkoutHeader.UserId);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }
         

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
