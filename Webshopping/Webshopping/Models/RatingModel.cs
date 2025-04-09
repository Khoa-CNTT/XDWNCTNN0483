using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Webshopping.Models
{
    public class RatingModel
    {
        [Key]
        public int Id { get; set; } // Đây là khoá chính
        
        public int ProductId { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập đánh giá sản phẩm"), MinLength(4)]
        public string Comment { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập tên sản phẩm"), MinLength(4)]
        public string Name { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập Email sản phẩm"), MinLength(4)]
        public string Email { get; set; }

        public string Star { get; set; }

        // Navigation property
        [ForeignKey("ProductId")]
        public ProductModel Product { get; set; }
        
    }
}
