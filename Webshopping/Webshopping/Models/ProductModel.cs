using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace Webshopping.Models
{
    public class ProductModel
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập tên sản phẩm"), MinLength(4)]
        public string Name { get; set; }
        public string Slug { get; set; }
        // [Required(ErrorMessage = "Yêu cầu nhập mô tả của sản phẩm"), MinLength(4)]
        public string Description { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập giá của sản phẩm")]
        [Range(1, double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn 0")]
        public decimal Price { get; set; }
        [Required(), Range(1, int.MaxValue, ErrorMessage = "Chọn thương hiệu")]
        public decimal CapitalPrice { get; set; }
        [Required(ErrorMessage="Yêu cầu nhập giá vốn sản phẩm")]
		public int BrandID { get; set; }
        [Required(), Range(1, int.MaxValue, ErrorMessage = "Chọn danh mục")]
        public int CategoryID { get; set; }
        public int Quantity { get; set; }
        public int Sold { get; set; }
        public CategoryModel Category { get; set; }
        public BrandModel Brand { get; set; }
        public RatingModel Ratings { get; set; }
        public string Img { get; set; }
        [NotMapped]
        public IFormFile ImageUpload { get; set; }
    }
}
