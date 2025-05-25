	using System.ComponentModel.DataAnnotations;
	using System.ComponentModel.DataAnnotations.Schema;
	using System.Runtime.CompilerServices;

	namespace Webshopping.Models
	{
		public class ContactModel
		{
			[Key]
			[Required(ErrorMessage = "Yêu cầu nhập tiêu đề website")]
			public string Name { get; set; }
			[Required(ErrorMessage = "Yêu cầu nhập bản đồ")]
			public string Map { get; set; }
			[Required(ErrorMessage = "Yêu cầu nhập số điện thoại")]
			public string Email{ get; set; }

			[Required(ErrorMessage = "Yêu cầu nhập email")]
			public string Phone { get; set; }

			[Required(ErrorMessage = "Yêu cầu nhập thông tin")]
			
			public string Description { get; set; }
			public string LogoImg { get; set; }

			[NotMapped]
			[FileExtensions]
			public IFormFile? ImageUpload { get; set; }
		}
	}
