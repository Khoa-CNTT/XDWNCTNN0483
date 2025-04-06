namespace Webshopping.Areas.Admin.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webshopping.Models;
using Webshopping.Repository;
using Webshopping.Areas.Admin.Common;
using Microsoft.AspNetCore.Authorization;

[Area("Admin")]
[Authorize]
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

    // GET: admin/category/update/{id}
    [HttpGet("category/update/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        CategoryModel existingCategory = await _dataContext.Categories.FindAsync(id);
        if (existingCategory == null)
        {
            return NotFound();
        }
        return View(existingCategory); // truyền model vào view
    }

    // POST: admin/category/update/{id}
    [HttpPost("category/update/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CategoryModel model)
    {
        if (ModelState.IsValid)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Tên danh mục không được để trống");
                return View(model);
            }

            // Tạo Slug
            model.Slug = string.IsNullOrWhiteSpace(model.Slug)
                ? SlugGenerate.GenerateSlug(model.Name)
                : model.Slug;

            // Kiểm tra Slug trùng (loại trừ chính nó)
            var slugExisted = await _dataContext.Categories
                .FirstOrDefaultAsync(p => p.Slug == model.Slug && p.Id != model.Id);
            if (slugExisted != null)
            {
                ModelState.AddModelError("", "Slug đã tồn tại trong database");
                return View(model);
            }

            // Tìm entity gốc từ DB
            var existingCategory = await _dataContext.Categories.FindAsync(model.Id);
            if (existingCategory == null)
            {
                return NotFound();
            }

            // Cập nhật từng trường
            existingCategory.Name = model.Name;
            existingCategory.Description = model.Description;
            existingCategory.Slug = model.Slug;
            existingCategory.Status = model.Status;

            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Danh mục đã được cập nhật thành công!";
            return RedirectToAction("Index");
        }

        // Xử lý lỗi
        List<string> errors = new List<string>();
        foreach (var value in ModelState.Values)
        {
            foreach (var error in value.Errors)
            {
                errors.Add(error.ErrorMessage);
            }
        }
        string errorMessage = string.Join("\n", errors);
        TempData["error"] = "Model có lỗi: " + errorMessage;
        return View(model);
    }

    // POST: admin/category/delete
    [HttpPost("category/delete")]
    public async Task<IActionResult> Delete(int id)
    {
        // Tìm sản phẩm theo ID
        var category = await _dataContext.Categories.FindAsync(id);

        // Kiểm tra nếu sản phẩm không tồn tại
        if (category == null)
        {
            TempData["error"] = "Danh mục không tồn tại!";
            return RedirectToAction("Index");
        }

        // Xóa sản phẩm khỏi cơ sở dữ liệu
        _dataContext.Categories.Remove(category);
        await _dataContext.SaveChangesAsync();

        TempData["success"] = "Danh mục đã được xóa thành công!";
        return RedirectToAction("Index");
    }
}
