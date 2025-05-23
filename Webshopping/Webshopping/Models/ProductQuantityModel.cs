using System;
using System.ComponentModel.DataAnnotations;

namespace Webshopping.Models
{
    public class ProductQuantityModel
    {

        public int Id { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập số lượng sản phẩm")]
        [Range(1, 2000, ErrorMessage = "Số lượng phải lớn hơn hoặc bằng 0 và nhỏ hơn hoặc bằng 2000")]
        public int Quantity { get; set; }
        public int ProductId { get; set; }
        public DateTime Date { get; set; }
    }
}
