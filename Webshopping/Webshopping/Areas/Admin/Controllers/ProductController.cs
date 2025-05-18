namespace Webshopping.Areas.Admin.Controllers;

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.ObjectPool;
using Webshopping.Areas.Admin.Common;
using Webshopping.Models;
using Webshopping.Repository;

[Area("Admin")]
[Authorize(Roles = "Employee,Admin")]
[Route("admin/product/")]
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
    [HttpGet("create")]
    public IActionResult Add()
    {
        ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name");
        ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name");
        return View();
    }

    // POST: /admin/product/create
    [HttpPost("create")]
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
                var randomNumber = random.Next(1, 9999);  // Sinh số ngẫu nhiên từ 1000 đến 9999
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

    [HttpGet("edit/{id}")]
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

    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductModel model)
    {
        var existingProduct = await _dataContext.Products.FindAsync(id);
        if (existingProduct == null)
        {
            return NotFound();
        }

        // Gán lại SelectList
        ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", model.CategoryID);
        ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", model.BrandID);

        // Nếu tên sản phẩm bị thay đổi, mới generate lại Slug và kiểm tra trùng
        if (!string.Equals(existingProduct.Name, model.Name, StringComparison.OrdinalIgnoreCase))
        {
            model.Slug = SlugGenerate.GenerateSlug(model.Name);

            var slugExists = await _dataContext.Products
                .AnyAsync(p => p.Slug == model.Slug && p.Id != id);

            if (slugExists)
            {
                ModelState.AddModelError("Slug", "Tên sản phẩm đã tồn tại trong hệ thống (trùng Slug).");
                return View(model);
            }

            existingProduct.Slug = model.Slug;
        }

        // Cập nhật các trường khác
        existingProduct.Name = model.Name;
        existingProduct.Description = model.Description;
        existingProduct.Price = model.Price;
        existingProduct.BrandID = model.BrandID;
        existingProduct.CategoryID = model.CategoryID;

        // Xử lý ảnh
        if (model.ImageUpload != null && model.ImageUpload.Length > 0)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(model.ImageUpload.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError("ImageUpload", "Tệp hình ảnh phải là .jpg, .jpeg, .png hoặc .gif.");
                return View(model);
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img");
            Directory.CreateDirectory(uploadsFolder);

            var random = new Random();
            var randomNumber = random.Next(1, 9999);  // Sinh số ngẫu nhiên từ 1000 đến 9999
            var fileName = $"img{randomNumber}{fileExtension}"; // Tên tệp mới là img{random số}{extension}

            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.ImageUpload.CopyToAsync(stream);
            }

            existingProduct.Img = fileName;
        }
        else if (string.IsNullOrEmpty(existingProduct.Img))
        {
            existingProduct.Img = "default.png";
        }

        _dataContext.Products.Update(existingProduct);
        await _dataContext.SaveChangesAsync();

        TempData["success"] = "Cập nhật sản phẩm thành công!";
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
    //add quantity

    [HttpGet("AddQuantity")]
    public async Task<IActionResult> AddQuantity(int Id)
    {
        var productbyquantity = await _dataContext.ProductQuantities.Where(pq => pq.ProductId == Id).ToListAsync();
        ViewBag.ProductByQuantity = productbyquantity;
        ViewBag.Id = Id;
        return View();

    }
    [Route("StoreProductQuantity")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult StoreProductQuantity(ProductQuantityModel productQuantityModel)
    {
        // Get the product to update
        var product = _dataContext.Products.Find(productQuantityModel.ProductId);

        if (product == null)
        {
            return NotFound(); // Handle product not found scenario
        }
        product.Quantity += productQuantityModel.Quantity;

        productQuantityModel.Quantity = productQuantityModel.Quantity;
        productQuantityModel.ProductId = productQuantityModel.ProductId;
        productQuantityModel.Date = DateTime.Now;


        _dataContext.Add(productQuantityModel);
        _dataContext.SaveChangesAsync();
        TempData["success"] = "Thêm số lượng sản phẩm thành công";
        return RedirectToAction("AddQuantity", "Product", new { Id = productQuantityModel.ProductId });
    }

}