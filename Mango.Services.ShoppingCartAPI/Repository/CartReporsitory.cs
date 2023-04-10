using AutoMapper;
using Mango.Services.ShoppingCartAPI.DbContexts;
using Mango.Services.ShoppingCartAPI.Migrations;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.dtos;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.ShoppingCartAPI.Repository
{
    public class CartReporsitory : ICartReporsitory
    {
        private readonly ApplicationDbContext _db;
        private IMapper _mapper;

        public CartReporsitory(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        #region coupon
        public async Task<bool> ApplyCoupon(string userId, string couponCode)
        {
            CartHeader cartHeaderInDb = await _db.CartHeaders.FirstOrDefaultAsync(x => x.UserId == userId);
            cartHeaderInDb.CouponCode = couponCode;
            _db.CartHeaders.Update(cartHeaderInDb);
            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<bool> RemoveCoupon(string userId)
        {
            CartHeader cartHeaderInDb = await _db.CartHeaders.FirstOrDefaultAsync(x => x.UserId == userId);
            cartHeaderInDb.CouponCode = string.Empty;
            _db.CartHeaders.Update(cartHeaderInDb);
            await _db.SaveChangesAsync();
            return true;
        }
        #endregion
        public async Task<bool> ClearCart(string userId)
        {
            CartHeader cartHeaderInDb = await _db.CartHeaders.FirstOrDefaultAsync(x => x.UserId == userId);
            if (cartHeaderInDb != null)
            {
                _db.CartDetails.RemoveRange(_db.CartDetails.Where(x => x.CartHeaderId == cartHeaderInDb.CartHeaderId));
                _db.CartHeaders.Remove(cartHeaderInDb);
                await _db.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<CartDto> CreateUpdateCart(CartDto cartDto)
        {
            Cart cart = _mapper.Map<Cart>(cartDto);
            try
            { 
                //check if product exists in database, if not create it!
                var prodInDb = await _db.Products
                    .FirstOrDefaultAsync(u => u.ProductId == cartDto.CartDetails.FirstOrDefault()
                    .ProductId);
                if (prodInDb == null)
                {
                    _db.Products.Add(cart.CartDetails.FirstOrDefault().Product);
                    await _db.SaveChangesAsync();
                }


                //check if header is null
                var cartHeaderFromDb = await _db.CartHeaders.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserId == cart.CartHeader.UserId);

                if (cartHeaderFromDb == null)
                {
                    //create header and details
                    _db.CartHeaders.Add(cart.CartHeader);
                    await _db.SaveChangesAsync();
                    cart.CartDetails.FirstOrDefault().CartHeaderId = cart.CartHeader.CartHeaderId;
                    cart.CartDetails.FirstOrDefault().CartHeader = await _db.CartHeaders.FirstOrDefaultAsync(x => x.CartHeaderId == cart.CartHeader.CartHeaderId); //extra para
                    cart.CartDetails.FirstOrDefault().Product = null;
                    _db.CartDetails.Add(cart.CartDetails.FirstOrDefault());
                    await _db.SaveChangesAsync();
                }
                else
                {
                    //if header is not null
                    //check if details has same product
                    var cartDetailsFromDb = await _db.CartDetails.AsNoTracking().FirstOrDefaultAsync(
                        u => u.ProductId == cart.CartDetails.FirstOrDefault().ProductId &&
                        u.CartHeaderId == cartHeaderFromDb.CartHeaderId);

                    if (cartDetailsFromDb == null)
                    {
                        //create details
                        cart.CartDetails.FirstOrDefault().CartHeaderId = cartHeaderFromDb.CartHeaderId;
                        cart.CartDetails.FirstOrDefault().CartHeader = await _db.CartHeaders.FirstOrDefaultAsync(x => x.CartHeaderId == cartHeaderFromDb.CartHeaderId); //extra para
                        cart.CartDetails.FirstOrDefault().Product = null;
                        _db.CartDetails.Add(cart.CartDetails.FirstOrDefault());
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        //update the count / cart details
                        cart.CartDetails.FirstOrDefault().Product = null;

                        cart.CartDetails.FirstOrDefault().Count += cartDetailsFromDb.Count;
                        cart.CartDetails.FirstOrDefault().CartDetailsId = cartDetailsFromDb.CartDetailsId;
                        cart.CartDetails.FirstOrDefault().CartHeaderId = cartDetailsFromDb.CartHeaderId;

                        cart.CartDetails.FirstOrDefault().CartHeader = await _db.CartHeaders.FirstOrDefaultAsync(x => x.CartHeaderId == cartHeaderFromDb.CartHeaderId); //extra para
                        _db.CartDetails.Update(cart.CartDetails.FirstOrDefault());
                        await _db.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return _mapper.Map<CartDto>(cart);
        }

        public async Task<CartDto> GetCartByUserId(string userId)
        {

            Cart cart = new()
            {
                CartHeader = await _db.CartHeaders.FirstOrDefaultAsync(u => u.UserId == userId)
            };

            cart.CartDetails = _db.CartDetails.Where(x => x.CartHeaderId == cart.CartHeader.CartHeaderId).Include(x => x.Product);

            return _mapper.Map<CartDto>(cart);
        }

     

        public async Task<bool> RemoveFromCart(int cartDetailsId)
        {
            try
            {
                var cartDetails = await _db.CartDetails.FirstOrDefaultAsync(x => x.CartDetailsId == cartDetailsId);
                var totalCount = _db.CartDetails.Where(x => x.CartHeaderId == cartDetails.CartHeaderId).Count();
                _db.CartDetails.Remove(cartDetails);
                if (totalCount == 1)
                {
                    var cartHeaderToRemove = await _db.CartHeaders.FirstOrDefaultAsync(x => x.CartHeaderId == cartDetails.CartHeaderId);
                    _db.CartHeaders.Remove(cartHeaderToRemove);
                }

                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex) { return false; }
        }
    }
}
