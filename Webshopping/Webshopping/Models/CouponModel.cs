using System.ComponentModel.DataAnnotations;

namespace Webshopping.Models
{
    public class CouponModel
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập tên mã giảm giá")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Trường không được bỏ trống")]
        public string description { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateExpire { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập số lượng mã giảm giá")]
        [Range(1, 1000, ErrorMessage = "Số lượng phải nằm trong khoảng từ 1 đến 1000")]
        public int Quantity { get; set; }

        public int Status { get; set; } // 0: chưa sử dụng, 1: đã sử dụng
        public int DiscountType { get; set; } // 0: phần trăm, 1: giá trị cố định
        public decimal DiscountValue { get; set; } // ví dụ: 10% => 10 | hoặc 50.000đ


    }
}
