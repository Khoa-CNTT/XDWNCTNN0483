namespace Webshopping.Areas.Amdin.Controllers;

using System.Globalization;
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
    private readonly IWebHostEnvironment _webHostEnvironment;
    public ProductController(DataContext context, IWebHostEnvironment webHostEnvironment)
    {
        _dataContext = context;
        _webHostEnvironment = webHostEnvironment;
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

    // POST: /admin/product/create
    [HttpPost("product/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(ProductModel model)
    {
        ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", model.CategoryID);
        ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", model.BrandID);

        // Kiểm tra và gán giá trị mặc định cho Img nếu không có ảnh
        if (model.ImageUpload != null)
        {
            model.Slug = model.Name.Replace(" ", "-");
            var slug = await _dataContext.Products.FirstOrDefaultAsync(p => p.Slug == model.Slug);

            if (slug != null)
            {
                ModelState.AddModelError("", "Sản phẩm đã có trong database");
                return View(model);
            }
            else
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = System.IO.Path.GetExtension(model.ImageUpload.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ImageUpload", "Tệp hình ảnh phải có định dạng .jpg, .jpeg, .png hoặc .gif.");
                    return View(model);
                }

                // Đường dẫn lưu ảnh
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img");

                // Kiểm tra và tạo thư mục nếu không tồn tại
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Sinh ra số ngẫu nhiên 4 chữ số
                var random = new Random();
                var randomNumber = random.Next(1000, 9999);  // Sinh số ngẫu nhiên từ 1000 đến 9999
                var fileName = $"img{randomNumber}{fileExtension}"; // Tên tệp mới là img{random số}{extension}

                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageUpload.CopyToAsync(stream);
                }
                model.Img = fileName;  // Chỉ lưu tên tệp vào cơ sở dữ liệu
            }
        }
        else
        {
            model.Img = "1.jpg";  // Gán ảnh mặc định nếu không có ảnh tải lên
        }

        // Thêm sản phẩm vào cơ sở dữ liệu
        _dataContext.Products.Add(model);
        await _dataContext.SaveChangesAsync();

        TempData["success"] = "Sản phẩm đã được thêm thành công!";
        return RedirectToAction("Index");
    }

    [HttpGet("product/edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        // Fetch the product from the database
        ProductModel product = await _dataContext.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound(); // Return 404 if the product is not found
        }

        // Populate dropdowns for categories and brands
        ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryID);
        ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandID);

        return View(product); // Return the view with the product data
    }

    [HttpPost("product/edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductModel model)
    {
        ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", model.CategoryID);
        ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", model.BrandID);

        // Kiểm tra và gán giá trị mặc định cho Img nếu không có ảnh
        if (model.ImageUpload != null)
        {
            model.Slug = model.Name.Replace(" ", "-");
            var slug = await _dataContext.Products.FirstOrDefaultAsync(p => p.Slug == model.Slug);

            if (slug != null)
            {
                ModelState.AddModelError("", "Sản phẩm đã có trong database");
                return View(model);
            }
            else
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = System.IO.Path.GetExtension(model.ImageUpload.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ImageUpload", "Tệp hình ảnh phải có định dạng .jpg, .jpeg, .png hoặc .gif.");
                    return View(model);
                }

                // Đường dẫn lưu ảnh
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img");

                // Kiểm tra và tạo thư mục nếu không tồn tại
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Sinh ra số ngẫu nhiên 4 chữ số
                var random = new Random();
                var randomNumber = random.Next(1000, 9999);  // Sinh số ngẫu nhiên từ 1000 đến 9999
                var fileName = $"img{randomNumber}{fileExtension}"; // Tên tệp mới là img{random số}{extension}

                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageUpload.CopyToAsync(stream);
                }
                model.Img = fileName;  // Chỉ lưu tên tệp vào cơ sở dữ liệu
            }
        }
        else
        {
            model.Img = "1.jpg";  // Gán ảnh mặc định nếu không có ảnh tải lên
        }

        // Thêm sản phẩm vào cơ sở dữ liệu
        _dataContext.Products.Update(model);
        await _dataContext.SaveChangesAsync();

        TempData["success"] = "Sản phẩm đã được thêm thành công!";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        // Tìm sản phẩm theo ID
        var product = await _dataContext.Products.FindAsync(id);

        // Kiểm tra nếu sản phẩm không tồn tại
        if (product == null)
        {
            TempData["error"] = "Sản phẩm không tồn tại!";
            return RedirectToAction("Index");
        }

        // Xóa hình ảnh từ thư mục nếu sản phẩm có hình ảnh
        if (!string.IsNullOrEmpty(product.Img))
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", product.Img);

            if (System.IO.File.Exists(filePath))
            {
                // Xóa tệp hình ảnh
                System.IO.File.Delete(filePath);
            }
        }

        // Xóa sản phẩm khỏi cơ sở dữ liệu
        _dataContext.Products.Remove(product);
        await _dataContext.SaveChangesAsync();

        TempData["success"] = "Sản phẩm đã được xóa thành công!";
        return RedirectToAction("Index");
    }
}