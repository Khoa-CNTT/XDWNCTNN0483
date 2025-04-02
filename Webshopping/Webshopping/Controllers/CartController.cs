using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;
using Webshopping.Models;
using Webshopping.Models.ViewModel;
using Webshopping.Repository;
using Webshopping.Repository.Shopping_Tutorial.Repository;

namespace Webshopping.Controllers
{
    public class CartController : Controller
    {
        private readonly DataContext _datacontext;
        public CartController(DataContext _context)
        {
            _datacontext = _context;
        }
        public IActionResult Index()
        {
            List<CartItemModels> cartItem = HttpContext.Session.GetJson<List<CartItemModels>>("cart") ?? new List<CartItemModels>();
            CartItemViewModel cartVM = new()
            {
                CartItems = cartItem,
                GrandTotal = cartItem.Sum(x => x.Price * x.Quantity)
            };
            return View(cartVM);
        }
        public IActionResult Checkout()
        {
            return View("~/Views/Checkout/Index.cshtml");
        }
        public async Task<IActionResult> Add(int Id)
        {
            ProductModel product = await _datacontext.Products.FindAsync(Id);
            List<CartItemModels> cart = HttpContext.Session.GetJson<List<CartItemModels>>("cart") ?? new List<CartItemModels>();
            CartItemModels cartItems = cart.Where(c => c.ProductId == Id).FirstOrDefault();

            if (cartItems == null)
            {
                cart.Add(new CartItemModels(product));
            }
            else
            {
                cartItems.Quantity++;
            }
            HttpContext.Session.SetJson("cart", cart);
            return Redirect(Request.Headers["Referer"].ToString());

        }
    }
}
