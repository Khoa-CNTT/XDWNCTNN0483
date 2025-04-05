namespace Webshopping.Areas.Admin.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webshopping.Models;
using Webshopping.Repository;
using Webshopping.Areas.Admin.Common;

[Area("Admin")]
[Route("admin/")]
public class CategoryController : Controller
{
    private readonly DataContext _dataContext;

    public CategoryController(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    // GET: admin/category
    [HttpGet("category")]
    public async Task<IActionResult> Index()
    {
        return View(await _dataContext.Categories.OrderByDescending(p => p.Id).ToListAsync());
    }

    //GET: admin/category/create
    [HttpGet("category/create")]
    public IActionResult Add()
    {
        return View();
    }

    [HttpPost("category/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(CategoryModel model)
    {
        if (ModelState.IsValid)
        {
            // Kiểm tra xem Name có hợp lệ không
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Tên danh mục không được để trống");
                return View(model);
            }

            // Tạo Slug từ Name sử dụng SlugGenerate
            model.Slug = string.IsNullOrWhiteSpace(model.Slug) ? SlugGenerate.GenerateSlug(model.Name) : model.Slug;

            // Kiểm tra xem Slug có tồn tại trong cơ sở dữ liệu không
            var slug = await _dataContext.Categories.FirstOrDefaultAsync(p => p.Slug == model.Slug);
            if (slug != null)
            {
                ModelState.AddModelError("", "Danh mục đã có trong database");
                return View(model); // Trả về model khi có lỗi
            }

            // Thêm danh mục vào cơ sở dữ liệu
            _dataContext.Categories.Add(model);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Danh mục đã được thêm thành công!";
            return RedirectToAction("Index");
        }
        else
        {
            // Hiển thị các lỗi nếu có
            List<string> errors = new List<string>();
            foreach (var value in ModelState.Values)
            {
                foreach (var error in value.Errors)
                {
                    errors.Add(error.ErrorMessage);
                }
            }
            string errorMessage = string.Join("\n", errors);
            TempData["error"] = "Model có một vài thứ đang lỗi: " + errorMessage;
            return View(model); // Trả về view với lỗi
        }
    }
}