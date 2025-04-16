using System.Diagnostics;
using System.Threading.Tasks;
using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webshopping.Models;
using Webshopping.Repository;

namespace Webshopping.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AppUserModel> _userManager;

        public HomeController(ILogger<HomeController> logger, DataContext context, UserManager<AppUserModel> userManager)
        {
            _logger = logger;
            _dataContext = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var products = _dataContext.Products.Include("Category").Include("Brand").ToList();

            var sliders = _dataContext.Slider.Where(s => s.Status == 1).ToList();
            ViewBag.Slider = sliders;
			
            return View(products);

        }

        public async Task<IActionResult> Compare()
        {
            var compare_product = await (from c in _dataContext.Compares
                                         join p in _dataContext.Products on c.ProductId equals p.Id
                                         join u in _dataContext.Users on c.UserId equals u.Id
                                         select new { User = u, Compares = c, Product = p })
                                         .ToListAsync(); // Fixed: Added ToListAsync() to execute the query asynchronously
            return View(compare_product);
        }
        public async Task<IActionResult> DeleteCompare(int Id)
        {
            CompareModel compare = await _dataContext.Compares.FindAsync(Id);

            _dataContext.Compares.Remove(compare);

            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Sản phẩm so sánh đã được xóa thành công";
            return RedirectToAction("Compare", "Home");
        }
        public async Task<IActionResult> Wishlist()
        {
            var wishlist_product = await (from w in _dataContext.Wishlists
                                         join p in _dataContext.Products on w.ProductId equals p.Id
                                         select new { Wishlists = w, Product = p })
                                         .ToListAsync(); // Fixed: Added ToListAsync() to execute the query asynchronously
            return View(wishlist_product);
        }
        public async Task<IActionResult> DeleteWishlist(int Id)
        {
            WishlistModel Wishlist = await _dataContext.Wishlists.FindAsync(Id);

            _dataContext.Wishlists.Remove(Wishlist);

            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Sản phẩm yêu thích đã được xóa thành công";
            return RedirectToAction("Wishlist", "Home");
        }

        public async Task<IActionResult> AddWishlist(int Id, WishlistModel wishlist)
        {
            var user = await _userManager.GetUserAsync(User);
            wishlist.ProductId = Id;
            wishlist.UserId = user.Id;
            _dataContext.Wishlists. Add(wishlist);
            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "thêm yêu thích thành công" });
            }
            catch (Exception)
            {
                return StatusCode(500, "lỗi thêm vào danh sách yêu thích ");
            }   
        }

        public async Task<IActionResult> AddCompare(int Id, CompareModel compare)
        {
            var user = await _userManager.GetUserAsync(User);
            compare.ProductId = Id;
            compare.UserId = user.Id;
            _dataContext.Compares.Add(compare);
            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "thêm sản phẩm so sánh thành công" });
            }
            catch (Exception)
            {
                return StatusCode(500, "lỗi thêm  sản phẩm so sánh ");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

		public IActionResult Contact()
		{
			return View();

		}
		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int statuscode)
        {
            if (statuscode == 404)
            {
                return View("NotFound");
            }
            else if (statuscode == 500)
            {
                return View("ServerError");
            }
            else
            {
                return View("Error");
            }
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
