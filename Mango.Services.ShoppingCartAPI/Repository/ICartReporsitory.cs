using Mango.Services.ShoppingCartAPI.Models.dtos;

namespace Mango.Services.ShoppingCartAPI.Repository
{
    public interface ICartReporsitory
    {
        Task<CartDto> GetCartByUserId(string userId);
        Task<CartDto> CreateUpdateCart(CartDto cartDto);
        Task<bool> RemoveFromCart(int cartDetailsId);
        Task<bool> ClearCart(string userId);
    }
}
