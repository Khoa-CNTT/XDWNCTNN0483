namespace Webshopping.Areas.Admin.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Webshopping.Areas.Admin.Common;
using Webshopping.Models;
using Webshopping.Repository;

[Area("Admin")]
[Route("admin/brand")]
public class BrandController : Controller
{
    private readonly DataContext _dataContext;

    public BrandController(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    // GET: admin/brand 
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        // xử lí đồng bộ database 
        return View(await _dataContext.Brands
                .OrderByDescending((brand) => brand.Id)
                .ToListAsync());
    }

    //GET: admin/brand/create
    [HttpGet("create")]
    public IActionResult Add()
    {
        return View();
    }

    //GET: admin/brand/create
    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(BrandModel model)
    {
        if (ModelState.IsValid)
        {
            // Name thương hiệu không dược để trống
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Tên thương hiệu không được để trống!");
                return View(model);
            }

            // Slug không được để trống
            model.Slug = string.IsNullOrWhiteSpace(model.Slug) ? SlugGenerate.GenerateSlug(model.Name) : model.Slug;

            // Kiểm tra trong cơ sở dữ liệu có Slug chưa
            var slug = await _dataContext.Brands.FirstOrDefaultAsync((brand) => brand.Slug == model.Slug);
            if (slug != null)
            {
                ModelState.AddModelError("", "Slug đã có trong cơ sở dữ liệu");
                return View(model); // trả về trang lỗi
            }

            // thêm thương hiệu vào database
            _dataContext.Brands.Add(model);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Thêm thương hiệu thành công";
            return RedirectToAction("Index");
        }
        else
        {
            List<string> errors = new List<string>();
            foreach (var value in ModelState.Values)
            {
                foreach (var error in errors)
                {
                    errors.Add(error);
                }
            }
            string errorMessage = string.Join("\n", errors);
            TempData["error"] = "Model có một vài thứ đang lỗi: " + errorMessage;
            return View(model); // trả về trang lỗi
        }
    }
}