using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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
        public IActionResult Index(ShippingModel shippingModel)
        {
            List<CartItemModels> cartItems = HttpContext.Session.GetJson<List<CartItemModels>>("Cart") ?? new List<CartItemModels>();
            // Nhận shipping giá từ cookie
            var shippingPriceCookie = Request.Cookies["ShippingPrice"];
            decimal shippingPrice = 0;

            if (shippingPriceCookie != null)
            {
                var shippingPriceJson = shippingPriceCookie;
                shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);
            }
            CartItemViewModel cartVM = new()
            {
                CartItems = cartItems,
                GrandTotal = cartItems.Sum(x => x.Quantity * x.Price),
                ShippingPrice = shippingPrice
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

        [HttpPost]
        [Route("Cart/GetShipping")]
        public async Task<IActionResult> GetShipping(ShippingModel shippingModel, string quan, string tinh, string phuong)
        {

            var existingShipping = await _datacontext.Shippings
                .FirstOrDefaultAsync(x => x.City == tinh && x.Distric == quan && x.Ward == phuong);

            decimal shippingPrice = 0; // Set mặc định giá tiền

            if (existingShipping != null)
            {
                shippingPrice = existingShipping.Price;
            }
            else
            {
                //Set mặc định giá tiền nếu ko tìm thấy
                shippingPrice = 50000;
            }
            var shippingPriceJson = JsonConvert.SerializeObject(shippingPrice);
            try
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30),
                    Secure = true // using HTTPS
                };

                Response.Cookies.Append("ShippingPrice", shippingPriceJson, cookieOptions);
            }
            catch (Exception ex)
            {
                //
                Console.WriteLine($"Error adding shipping price cookie: {ex.Message}");
            }
            return Json(new { shippingPrice });
        }

        [HttpPost]
        [Route("Cart/RemoveShippingCookie")]
        public IActionResult RemoveShippingCookie()
        {
            Response.Cookies.Delete("ShippingPrice");
            return RedirectToAction("Index", "Cart");
        }
    }
}
