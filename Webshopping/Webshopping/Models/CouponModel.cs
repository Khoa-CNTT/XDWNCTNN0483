using System.ComponentModel.DataAnnotations;

namespace Webshopping.Models
{
    public class CouponModel
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập tên mã giảm giá")]
        public string name { get; set; }
        [Required(ErrorMessage = "Trường không được bỏ trống")]
        public string description { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateExpire { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập % giảm giá")]
        public Decimal Price { get; set; } // giá trị giảm giá
        [Required(ErrorMessage = "Yêu cầu nhập số lượng coupon")]
        public int Quantity { get; set; }
        public int Status { get; set; } // 0: không hoạt động, 1: hoạt động

    }
}
