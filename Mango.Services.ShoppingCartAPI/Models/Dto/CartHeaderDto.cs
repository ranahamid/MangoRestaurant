using System.ComponentModel.DataAnnotations;

namespace Mango.Services.ShoppingCartAPI.Models.dtos
{
    public class CartHeaderDto
    {
        [Key]
        public int CartHeaderId { get; set; }
        public string UserId { get; set; }
        public string CouponCode { get; set;}
    }
}
