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
            var product = _dataContext.Products
        .Include(p => p.Category)
        .Include(p => p.Brand)
        .FirstOrDefault(p => p.Id == Id);

            if (product == null)
            {
                return NotFound();
            }

            var viewModel = new ProductDetailsViewModel
            {
                ProductDetails = product
            };

            ViewBag.RelatedProducts = _dataContext.Products
                .Where(p => p.CategoryID == product.CategoryID && p.Id != product.Id)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .ToList();

            return View(viewModel);
        }

        public async Task<IActionResult> CommentProduct(RatingModel rating)
        {
            if (ModelState.IsValid)
            {
                
                var ratingEntity = new RatingModel
                {
                    ProductId = rating.ProductId,
                    Name = rating.Name,
                    Email = rating.Email,
                    Comment = rating.Comment,
                    Star = rating.Star

                };

                _dataContext.Ratings.Add(ratingEntity);
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Thêm đánh giá thành công";

                return Redirect(Request.Headers["Referer"]);
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

                return RedirectToAction("Detail", new { id = rating.ProductId });
            }

            return Redirect(Request.Headers["Referer"]);
        }

    }

}



