using System.ComponentModel.DataAnnotations;

namespace Webshopping.Models
{
    public class CategoryModel
    {
        [Key]
        public int Id { get; set; }
        [Required, MinLength(4, ErrorMessage ="yêu cầu nhập tên")]
        public string Name { get; set; }
        [Required, MinLength(4, ErrorMessage ="Yêu cầu nhập mô tả")]
        public string Description { get; set; }
        [Required]
        public string Slug {  get; set; }
        public int Status { get; set; }
    }
}
