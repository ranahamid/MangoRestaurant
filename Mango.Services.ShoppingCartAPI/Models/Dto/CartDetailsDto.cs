using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mango.Services.ShoppingCartAPI.Models.dtos
{
    public class CartDetailsDto
    {
        [Key]
        public int CartDetailsId { get; set; }
        public int CartHeaderId { get; set; }
        [ForeignKey("CartHeaderId")]
        public virtual CartHeaderDto CartHeader { get;set; } = new CartHeaderDto();
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual ProductDto Product { get; set; } = new ProductDto();
        public int Count { get; set;  }
    }
}
