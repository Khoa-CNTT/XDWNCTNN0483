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

		public async Task<IActionResult> Index(string Slug)
		{
			BrandModel brand = _dataContext.Brands.Where(p => p.Slug == Slug).FirstOrDefault();

			if (brand == null) return RedirectToAction("Index");

			var productsByBrand = _dataContext.Products.Where(p => p.BrandID == brand.Id);
			ViewBag.Slug = Slug;
			return View(await productsByBrand.OrderByDescending(p => p.Id).ToListAsync());
		}
	}
}