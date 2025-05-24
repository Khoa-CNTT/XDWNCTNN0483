namespace Webshopping.Areas.Admin.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webshopping.Models;
using Webshopping.Repository;
using Webshopping.Areas.Admin.Common;
using Microsoft.AspNetCore.Authorization;
using Webshopping.Areas.Admin.Service;

[Area("Admin")]
[Route("admin/category/")]
[Authorize(Roles = "Employee,Admin")]
public class CategoryController : Controller
{
    private readonly DataContext _dataContext;
    private readonly IExcelImportService _excelImportService;

    public CategoryController(DataContext context, IExcelImportService excelImportService)
    {
        _dataContext = context;
        _excelImportService = excelImportService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(int pg = 1)
    {
        var brands = _dataContext.Categories.ToList();
        return View(brands);
    }
    [HttpGet("create")]
    public IActionResult Add()
    {
        return View();
    }
    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(CategoryModel category)
    {
        if (ModelState.IsValid)
        {
            category.Slug = category.Name.Replace(" ", "-");
            var slug = await _dataContext.Categories.FirstOrDefaultAsync(p => p.Slug == category.Slug);
            if (slug != null)
            {
                ModelState.AddModelError("", "Danh mục đã có trong database");
                return View(category);
            }

            _dataContext.Add(category);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Thêm danh mục thành công";
            return RedirectToAction("Index");

        }
        else
        {
            TempData["error"] = "Model có một vài thứ đang lỗi";
            List<string> errors = new List<string>();
            foreach (var value in ModelState.Values)
            {
                foreach (var error in value.Errors)
                {
                    errors.Add(error.ErrorMessage);
                }
            }
            string errorMessage = string.Join("\n", errors);
            return BadRequest(errorMessage);
        }
        return View(category);
    }

    [HttpGet("edit/{Id}")]
    public async Task<IActionResult> Edit(int Id)
    {
        CategoryModel category = await _dataContext.Categories.FindAsync(Id);
        return View(category);
    }

    [HttpPost("edit/{Id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CategoryModel category)
    {
        if (ModelState.IsValid)
        {
            category.Slug = category.Name.Replace(" ", "-");

            _dataContext.Update(category);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Cập nhật danh mục thành công";
            return RedirectToAction("Index");
        }
        else
        {
            TempData["error"] = "Model có một vài thứ đang lỗi";
            List<string> errors = new List<string>();
            foreach (var value in ModelState.Values)
            {
                foreach (var error in value.Errors)
                {
                    errors.Add(error.ErrorMessage);
                }
            }
            string errorMessage = string.Join("\n", errors);
            return BadRequest(errorMessage);
        }
        return View(category);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        // Tìm sản phẩm theo ID
        var catagory = await _dataContext.Categories.FindAsync(id);

        // Kiểm tra nếu sản phẩm không tồn tại
        if (catagory == null)
        {
            TempData["error"] = "Danh mục không tồn tại!";
            return RedirectToAction("Index");
        }

        // Xóa sản phẩm khỏi cơ sở dữ liệu
        _dataContext.Categories.Remove(catagory);
        await _dataContext.SaveChangesAsync();

        TempData["success"] = "Danh mục đã được xóa thành công!";
        return RedirectToAction("Index");
    }

    [HttpPost("import-excel")]
    public IActionResult ImportExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError("", "Please select a valid Excel file.");
            return View(); // hoặc redirect
        }

        _excelImportService.ImportFromExcel(file);

        TempData["Success"] = "Import thành công!";
        return RedirectToAction("Index"); // chuyển đến trang danh sách
    }
}