namespace Webshopping.Areas.Amdin.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.ObjectPool;
using Webshopping.Models;
using Webshopping.Repository;

[Area("Admin")]
[Route("admin/")]
public class ProductController : Controller
{
    private readonly DataContext _dataContext;

    public ProductController(DataContext context)
    {
        _dataContext = context;
    }

    // GET: /admin/product
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        return View(await _dataContext.Products
            .OrderByDescending(p => p.Id)
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .ToListAsync());
    }

    // GET: /admin/product/create
    [HttpGet("product/create")]
    public IActionResult Add()
    {
        ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name");
        ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name");
        return View();
    }

    public async Task<IActionResult> Add(ProductModel model)
    {
        // model.CategoryID: Giá trị danh mục được chọn trước(nếu có) khi chỉnh sửa sản phẩm
        ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", model.CategoryID);

        // model.BrandID: Giá trị thương hiệu được chọn trước (nếu có) khi chỉnh sửa sản phẩm
        ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", model.BrandID);
        return View(model);
    }
}