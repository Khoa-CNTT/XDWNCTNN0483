using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Webshopping.Models;
using Webshopping.Repository;

namespace Webshopping.Controllers
{
	public class BrandController : Controller
	{
		private readonly DataContext _dataContext;

		public BrandController(DataContext context)
		{
			_dataContext = context;
		}

		public async Task<IActionResult> Index(string slug, string sort_by = "", string startprice = "", string endprice = "")
        {
            var brand = await _dataContext.Brands.FirstOrDefaultAsync(b => b.Slug == slug);

            if (brand == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.Slug = slug;

            IQueryable<ProductModel> productsByBrand = _dataContext.Products.Where(p => p.BrandID == brand.Id);

            // Lọc theo giá
            if (!string.IsNullOrEmpty(startprice) && !string.IsNullOrEmpty(endprice))
            {
                if (decimal.TryParse(startprice, out decimal startPriceValue) &&
                    decimal.TryParse(endprice, out decimal endPriceValue))
                {
                    productsByBrand = productsByBrand.Where(p => p.Price >= startPriceValue && p.Price <= endPriceValue);
                }
            }

            // Sắp xếp
            productsByBrand = sort_by switch
            {
                "price_increase" => productsByBrand.OrderBy(p => p.Price),
                "price_decrease" => productsByBrand.OrderByDescending(p => p.Price),
                "price_newest" => productsByBrand.OrderByDescending(p => p.Id),
                "price_oldest" => productsByBrand.OrderBy(p => p.Id),
                _ => productsByBrand.OrderByDescending(p => p.Id),
            };

            int count = await productsByBrand.CountAsync();
            ViewBag.count = count;
            ViewBag.sort_key = sort_by;

            if (count > 0)
            {
                decimal minPrice = await productsByBrand.MinAsync(p => p.Price);
                decimal maxPrice = await productsByBrand.MaxAsync(p => p.Price);

                ViewBag.minprice = minPrice;
                ViewBag.maxprice = maxPrice;
            }

            return View(await productsByBrand.ToListAsync());
        }
    }
	
}