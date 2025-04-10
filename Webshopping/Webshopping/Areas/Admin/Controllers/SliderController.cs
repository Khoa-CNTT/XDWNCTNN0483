using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Webshopping.Areas.Admin.Common;
using Webshopping.Migrations;
using Webshopping.Models;
using Webshopping.Repository;

namespace Webshopping.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Route("admin/slider/")]
	[Authorize]
	public class SliderController : Controller
	{

		private readonly DataContext _dataContext;
		private readonly IWebHostEnvironment _webHostEnvironment;

		public SliderController(DataContext context, IWebHostEnvironment webHostEnvironment)
		{
			_dataContext = context;
			_webHostEnvironment = webHostEnvironment;
		}

		[HttpGet("")]
		public async Task<IActionResult> Index()
		{
			var sliders = await _dataContext.Sliders.OrderByDescending(s => s.Id).ToListAsync();
			return View(sliders);

		}

		[HttpGet("create")]
		public IActionResult Add()
		{
			return View();
		}

		[HttpPost("create")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Add(SliderModel slider)
		{
			if (slider.ImageUpload != null)
			{
				var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
				var fileExtension = Path.GetExtension(slider.ImageUpload.FileName).ToLower();

				if (!allowedExtensions.Contains(fileExtension))
				{
					ModelState.AddModelError("ImageUpload", "Tệp hình ảnh phải có định dạng .jpg, .jpeg, .png hoặc .gif.");
					return View(slider);
				}

				// Thư mục lưu ảnh: wwwroot/img/slider
				var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "slider");
				if (!Directory.Exists(uploadsFolder))
				{
					Directory.CreateDirectory(uploadsFolder);
				}

				// Tạo tên file ngẫu nhiên
				var random = new Random();
				var randomNumber = random.Next(1000, 9999);
				var fileName = $"slider_{randomNumber}{fileExtension}";
				var filePath = Path.Combine(uploadsFolder, fileName);

				// Lưu file vào ổ cứng
				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await slider.ImageUpload.CopyToAsync(stream);
				}

				// Lưu đường dẫn ảnh để sử dụng khi hiển thị (từ thư mục wwwroot)
				slider.Img = Path.Combine("img", "slider", fileName).Replace("\\", "/");
			}
			else
			{
				// Nếu không upload ảnh, dùng ảnh mặc định
				slider.Img = "img/slider/default.jpg"; // bạn có thể đặt ảnh mặc định ở đây
			}

			_dataContext.Sliders.Add(slider);
			await _dataContext.SaveChangesAsync();

			TempData["success"] = "Slider đã được thêm thành công!";
			return RedirectToAction("Index");
		}
		[HttpPost("edit/{id}")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, SliderModel slider)
		{
			var existingSlider = await _dataContext.Sliders.FindAsync(id);
			if (existingSlider == null)
			{
				return NotFound();
			}
			// Nếu có ảnh mới được upload
			if (slider.ImageUpload != null)
			{
				var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
				var fileExtension = Path.GetExtension(slider.ImageUpload.FileName).ToLower();

				if (!allowedExtensions.Contains(fileExtension))
				{
					ModelState.AddModelError("ImageUpload", "Tệp hình ảnh phải là .jpg, .jpeg, .png hoặc .gif.");
					return View(slider);
				}

				var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img","slider");
				if (!Directory.Exists(uploadsFolder))
				{
					Directory.CreateDirectory(uploadsFolder);
				}

				var random = new Random();
				var fileName = $"img{random.Next(1000, 9999)}{fileExtension}";
				var filePath = Path.Combine(uploadsFolder, fileName);

				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await slider.ImageUpload.CopyToAsync(stream);
				}

				existingSlider.Img = Path.Combine("img", "slider", fileName).Replace("\\", "/");

			}

			// Nếu không upload ảnh, giữ ảnh cũ
			// Nếu chưa có ảnh (Img rỗng), gán mặc định
			if (string.IsNullOrEmpty(existingSlider.Img))
			{
				existingSlider.Img = "1.jpg";
			}

			// Cập nhật các trường thông tin
			existingSlider.Name = slider.Name;
			existingSlider.Description = slider.Description;
			existingSlider.Status = slider.Status;
	

			_dataContext.Sliders.Update(existingSlider);
			await _dataContext.SaveChangesAsync();

			TempData["success"] = "Cập nhật slider thành công!";
			return RedirectToAction("Index");
		}
		[HttpPost]
		public async Task<IActionResult> Delete(int id)
		{
			// Tìm sản phẩm theo ID
			var slider = await _dataContext.Sliders.FindAsync(id);

			// Kiểm tra nếu sản phẩm không tồn tại
			if (slider == null)
			{
				TempData["error"] = "Slider không tồn tại!";
				return RedirectToAction("Index");
			}

			// Xóa hình ảnh từ thư mục nếu sản phẩm có hình ảnh
			if (!string.IsNullOrEmpty(slider.Img))
			{
				var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", slider.Img);

				if (System.IO.File.Exists(filePath))
				{
					// Xóa tệp hình ảnh
					System.IO.File.Delete(filePath);
				}
			}

			// Xóa sản phẩm khỏi cơ sở dữ liệu
			_dataContext.Sliders.Remove(slider);
			await _dataContext.SaveChangesAsync();

			TempData["success"] = "Slider đã được xóa thành công!";
			return RedirectToAction("Index");
		}
	}
}





