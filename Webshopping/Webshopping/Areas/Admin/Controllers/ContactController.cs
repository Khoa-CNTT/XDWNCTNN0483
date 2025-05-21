using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webshopping.Models;
using Webshopping.Repository;

namespace Webshopping.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Route("admin/contact/")]
	[Authorize(Roles = "Employee,Admin")]
	public class ContactController : Controller
	{
		private readonly DataContext _dataContext;
		private readonly IWebHostEnvironment _webHostEnvironment;

		public ContactController(DataContext dataContext, IWebHostEnvironment webHostEnvironment)
		{
			_dataContext = dataContext;
			_webHostEnvironment = webHostEnvironment;
		}

		[HttpGet("")]
		public IActionResult Index()
		{
			var contact = _dataContext.Contact.ToList();
			return View(contact);
		}

		[HttpGet("edit")]
		public async Task<IActionResult> Edit()
		{

			ContactModel contact = _dataContext.Contact.FirstOrDefault();
			if (contact == null)
			{
				return NotFound(); // Return 404 if the product is not found
			}
			return View(contact);
		}

		[HttpPost("edit")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(ContactModel contact)
		{
			var existingContact = _dataContext.Contact.FirstOrDefault();
			if (existingContact == null)
			{
				return NotFound();
			}
			// Nếu có ảnh mới được upload
			if (contact.ImageUpload != null)
			{
				var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
				var fileExtension = Path.GetExtension(contact.ImageUpload.FileName).ToLower();

				if (!allowedExtensions.Contains(fileExtension))
				{
					ModelState.AddModelError("ImageUpload", "Tệp hình ảnh phải là .jpg, .jpeg, .png hoặc .gif.");
					return View(contact);
				}

				var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Logo");
				if (!Directory.Exists(uploadsFolder))
				{
					Directory.CreateDirectory(uploadsFolder);
				}

				var random = new Random();
				var fileName = $"img{random.Next(1000, 9999)}{fileExtension}";
				var filePath = Path.Combine(uploadsFolder, fileName);

				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await contact.ImageUpload.CopyToAsync(stream);
				}

				existingContact.LogoImg = Path.Combine("img", "slider", fileName).Replace("\\", "/");

			}

			// Nếu không upload ảnh, giữ ảnh cũ
			// Nếu chưa có ảnh (Img rỗng), gán mặc định
			if (string.IsNullOrEmpty(existingContact.LogoImg))
			{
				existingContact.LogoImg = "1.jpg";
			}

			// Cập nhật các trường thông tin
			existingContact.Name = contact.Name;
			existingContact.Email = contact.Email;
			existingContact.Description = contact.Description;
			existingContact.Phone = contact.Phone;
			existingContact.Map = contact.Map;


			_dataContext.Contact.Update(existingContact);
			await _dataContext.SaveChangesAsync();

			TempData["success"] = "Cập nhật thành công!";
			return RedirectToAction("Index");
		}
	}

}