using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Webshopping.Models;
using Webshopping.Models.ViewModel;
using Webshopping.Repository;
using System.Drawing.Drawing2D;

namespace Webshopping.Controllers
{
	public class ProductController : Controller
	{
		
			private readonly DataContext _dataContext;
			public ProductController(DataContext context)
			{
				_dataContext = context;
			}
        public IActionResult Index()
		{
			return View();
		}
		public async Task<IActionResult> Details(int Id)
		{
			if (Id == null) return RedirectToAction("Index");

			var productsById = _dataContext.Products.Where(p => p.Id == Id).FirstOrDefault();

			return View(productsById);
		}

	}

}



