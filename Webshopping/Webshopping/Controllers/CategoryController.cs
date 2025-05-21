using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webshopping.Models;
using Webshopping.Repository;

namespace Webshopping.Controllers
{
    public class CategoryController : Controller
    {
        private readonly DataContext _dataContext;

        public CategoryController(DataContext context)
        {
            _dataContext = context;
        }



        public async Task<IActionResult> Index(string slug, string sort_by = "", string startprice = "", string endprice = "")
        {
            var category = await _dataContext.Categories.FirstOrDefaultAsync(c => c.Slug == slug);

            if (category == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.Slug = slug;

            IQueryable<ProductModel> productsByCategory = _dataContext.Products.Where(p => p.CategoryID == category.Id);

            // Lọc theo giá
            if (!string.IsNullOrEmpty(startprice) && !string.IsNullOrEmpty(endprice))
            {
                if (decimal.TryParse(startprice, out decimal startPriceValue) &&
                    decimal.TryParse(endprice, out decimal endPriceValue))
                {
                    productsByCategory = productsByCategory.Where(p => p.Price >= startPriceValue && p.Price <= endPriceValue);
                }
            }

            // Sắp xếp
            productsByCategory = sort_by switch
            {
                "price_increase" => productsByCategory.OrderBy(p => p.Price),
                "price_decrease" => productsByCategory.OrderByDescending(p => p.Price),
                "price_newest" => productsByCategory.OrderByDescending(p => p.Id),
                "price_oldest" => productsByCategory.OrderBy(p => p.Id),
                _ => productsByCategory.OrderByDescending(p => p.Id),
            };

            int count = await productsByCategory.CountAsync();
            ViewBag.count = count;
            ViewBag.sort_key = sort_by;

            if (count > 0)
            {
                decimal minPrice = await productsByCategory.MinAsync(p => p.Price);
                decimal maxPrice = await productsByCategory.MaxAsync(p => p.Price);

                ViewBag.minprice = minPrice;
                ViewBag.maxprice = maxPrice;
            }

            return View(await productsByCategory.ToListAsync());
        }
    }
}