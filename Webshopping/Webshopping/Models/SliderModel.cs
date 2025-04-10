using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Webshopping.Models
{
	public class SliderModel
	{
		public int Id { get; set; }
		[Required(ErrorMessage = "Yêu cầu nhập tên slider")]
		public string Name { get; set; }
		[Required(ErrorMessage = "Yêu cầu nhập mô tả của slider")]
		public string Description { get; set; }
		
		public int? Status { get; set; }
		public string Img { get; set; }
		
		[NotMapped]
		[FileExtensions]
		public IFormFile ImageUpload { get; set; }
	}
}
