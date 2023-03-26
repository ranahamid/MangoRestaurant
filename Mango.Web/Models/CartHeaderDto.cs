using System.ComponentModel.DataAnnotations;

namespace Mango.Web.Models
{
    public class CartHeaderDto
    { 
        public int CartHeaderId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string CouponCode { get; set;} = string.Empty;
        //details
        public double OrderTotal { get; set; }
    }
}
