using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            TempData["success"] = "Thêm sản phẩm vào giỏ hàng thành công";
            return Redirect(Request.Headers["Referer"].ToString());

        }
        public async Task<IActionResult> Decrease(int Id)
        {
            List<CartItemModels> cart = HttpContext.Session.GetJson<List<CartItemModels>>("cart");
            CartItemModels cartItems = cart.Where(c => c.ProductId == Id).FirstOrDefault();

            if (cartItems.Quantity > 1)
            {
                cartItems.Quantity--;
            }
            else
            {
                cart.RemoveAll(p => p.ProductId == Id);
            }
            if (cart.Count == 0)
            {
                HttpContext.Session.Remove("cart");
            }
            else
            {
                HttpContext.Session.SetJson("cart", cart);
            }
            TempData["success"] = "Giảm số lượng sản phẩm thành công";
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Increase(int Id)
        {
            ProductModel product = await _datacontext.Products.Where(p => p.Id == Id).FirstOrDefaultAsync();
            List<CartItemModels> cart = HttpContext.Session.GetJson<List<CartItemModels>>("cart");
            CartItemModels cartItems = cart.Where(c => c.ProductId == Id).FirstOrDefault();

            if (cartItems.Quantity >= 1 && product.Quantity > cartItems.Quantity )
            {
                cartItems.Quantity++;
                TempData["success"] = "Increase Product to cart Sucessfully! ";
            }
            else
            {
                cartItems.Quantity = product.Quantity;
                TempData["success"] = "Maximum Product Quantity to cart Sucessfully! ";
            }
            if (cart.Count == 0)
            {
                HttpContext.Session.Remove("cart");
            }
            else
            {
                HttpContext.Session.SetJson("cart", cart);
            }
            TempData["success"] = "Tăng số lượng sản phẩm thành công";
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Remove(int Id)
        {
            List<CartItemModels> cart = HttpContext.Session.GetJson<List<CartItemModels>>("cart");
            cart.RemoveAll(p => p.ProductId == Id);
            if (cart.Count == 0)
            {
                HttpContext.Session.Remove("cart");
            }
            else
            {
                HttpContext.Session.SetJson("cart", cart);
            }
            TempData["success"] = "Xóa sản phẩm thành công";
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Clear()
        {
            HttpContext.Session.Remove("cart");
            TempData["success"] = "Xóa giỏ hàng thành công";
            return RedirectToAction("Index");
        }
    }
}