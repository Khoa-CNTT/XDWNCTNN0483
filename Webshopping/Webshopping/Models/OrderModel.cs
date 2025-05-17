using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Webshopping.Models
{
    public class OrderModel
    {
        public int Id { get; set; }
        public string OrderCode { get; set; }
        [Precision(18, 2)]
        public decimal ShippingCost { get; set; }
        public string CouponCode { get; set; }
        public string UserName { get; set; }
        public DateTime CreateDate { get; set; }
        public int Status { get; set; }
        public string PaymentMethod { get; set; }
        [Precision(18, 2)]
        public decimal DiscountAmount { get; set; }
        [Precision(18, 2)]
        public decimal DiscountedPrice { get; set; }

        [ForeignKey("ProductId")]
        public ProductModel Product { get; set; }
    }
}