namespace Webshopping.Areas.Admin.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Webshopping.Areas.Admin.Common;
using Webshopping.Areas.Admin.Service;
using Webshopping.Models;
using Webshopping.Repository;

[Area("Admin")]
[Route("Admin/Brand")]
[Authorize(Roles = "Employee,Admin")]
public class BrandController : Controller
{
    private readonly DataContext _dataContext;
    private readonly IWebHostEnvironment _env;
    private readonly IExcelImportService _excelImportService;

    public BrandController(DataContext context, IExcelImportService excelImportService)
    {
        _dataContext = context;
        _excelImportService = excelImportService;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        var brands = _dataContext.Brands.ToList();
        return View(brands);
    }

    [HttpGet("create")]
    public IActionResult Add()
    {
        return View();
    }

    [HttpPost("import-excel")]
    public IActionResult ImportExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError("", "Hãy chọn excel.");
            return View(); // hoặc redirect
        }

        _excelImportService.ImportFromExcel(file);

        TempData["Success"] = "Import thành công!";
        return RedirectToAction("Index"); // chuyển đến trang danh sách
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(BrandModel brand)
    {
        if (ModelState.IsValid)
        {
            brand.Slug = brand.Name.Replace(" ", "-");
            var slug = await _dataContext.Brands.FirstOrDefaultAsync(p => p.Slug == brand.Slug);
            if (slug != null)
            {
                ModelState.AddModelError("", "Danh mục đã có trong database");
                return View(brand);
            }

            _dataContext.Add(brand);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Thêm thương hiệu thành công";
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
        return View(brand);
    }

    [HttpGet("edit")]
    public async Task<IActionResult> Edit(int Id)
    {
        BrandModel brand = await _dataContext.Brands.FindAsync(Id);
        return View(brand);
    }

    [HttpPost("edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BrandModel brand)
    {
        if (ModelState.IsValid)
        {
            brand.Slug = brand.Name.Replace(" ", "-");
            _dataContext.Update(brand);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Cập nhật thương hiệu thành công";
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
        return View(brand);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int Id)
    {
        BrandModel brand = await _dataContext.Brands.FindAsync(Id);

        _dataContext.Brands.Remove(brand);
        await _dataContext.SaveChangesAsync();
        TempData["success"] = "Thương hiệu đã được xóa thành công";
        return RedirectToAction("Index");
    }
}
