using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Webshopping.Models;
using Webshopping.Models.ViewModel;
using Webshopping.Repository;
using Webshopping.Repository.Webshopping.Repository;

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
            //Nhận Coupon code từ cookie
            var coupon_code = Request.Cookies["CouponTitle"];
            CartItemViewModel cartVM = new()
            {
                CartItems = cartItems,
                GrandTotal = cartItems.Sum(x => x.Quantity * x.Price),
                ShippingPrice = shippingPrice,
                CouponCode = coupon_code
            };

            return View(cartVM);
        }
        public IActionResult Checkout()
        {
            return View("~/Views/Checkout/Index.cshtml");
        }
        [HttpPost]
        public async Task<IActionResult> Add(int Id)
        {
            var product = await _datacontext.Products.FindAsync(Id);
            if (product == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm" });
            }

            var cart = HttpContext.Session.GetJson<List<CartItemModels>>("Cart") ?? new List<CartItemModels>();
            var cartItem = cart.FirstOrDefault(c => c.ProductId == Id);

            if (cartItem == null)
            {
                cart.Add(new CartItemModels(product));
            }
            else
            {
                cartItem.Quantity += 1;
            }

            HttpContext.Session.SetJson("Cart", cart);

            return Json(new { success = true, message = "Thêm giỏ hàng thành công" });
        }

        public async Task<IActionResult> Decrease(int Id)
        {
            List<CartItemModels> cart = HttpContext.Session.GetJson<List<CartItemModels>>("Cart");
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
                HttpContext.Session.Remove("Cart");
            }
            else
            {
                HttpContext.Session.SetJson("Cart", cart);
            }
            TempData["success"] = "Giảm số lượng sản phẩm thành công";
            return Redirect(Request.Headers["Referer"].ToString());
        }
        public async Task<IActionResult> Increase(int Id)
        {
            ProductModel product = await _datacontext.Products.Where(p => p.Id == Id).FirstOrDefaultAsync();
            List<CartItemModels> cart = HttpContext.Session.GetJson<List<CartItemModels>>("Cart");
            CartItemModels cartItems = cart.Where(c => c.ProductId == Id).FirstOrDefault();

            if (cartItems.Quantity >= 1 && product.Quantity > cartItems.Quantity)
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
                HttpContext.Session.Remove("Cart");
            }
            else
            {
                HttpContext.Session.SetJson("Cart", cart);
            }
            TempData["success"] = "Tăng số lượng sản phẩm thành công";
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Remove(int Id)
        {
            List<CartItemModels> cart = HttpContext.Session.GetJson<List<CartItemModels>>("Cart");
            cart.RemoveAll(p => p.ProductId == Id);
            if (cart.Count == 0)
            {
                HttpContext.Session.Remove("Cart");
            }
            else
            {
                HttpContext.Session.SetJson("Cart", cart);
            }
            TempData["success"] = "Xóa sản phẩm thành công";
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Clear()
        {
            HttpContext.Session.Remove("Cart");
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
        // GET: Coupon
        [HttpPost]
        [Route("Cart/GetCoupon")]
        public async Task<IActionResult> GetCoupon(CouponModel couponModel, string coupon_value)
        {
            var validCoupon = await _datacontext.Coupons
                .FirstOrDefaultAsync(x => x.Name == coupon_value);

            string couponTitle = validCoupon.Name + " | " + validCoupon?.description;

            if (couponTitle != null)
            {
                TimeSpan remainingTime = validCoupon.DateExpire - DateTime.Now;
                int daysRemaining = remainingTime.Days;

                //  Kiểm tra còn hạn và còn số lượng
                if (daysRemaining >= 0 && validCoupon.Quantity > 0)
                {
                    try
                    {
                        //  Trừ số lượng mã giảm
                        validCoupon.Quantity -= 1;
                        await _datacontext.SaveChangesAsync();

                        //  Lưu cookie
                        var cookieOptions = new CookieOptions
                        {
                            HttpOnly = true,
                            Expires = DateTimeOffset.UtcNow.AddMinutes(30),
                            Secure = true,
                            SameSite = SameSiteMode.Strict
                        };

                        Response.Cookies.Append("CouponTitle", couponTitle, cookieOptions);
                        return Ok(new { success = true, message = "Áp mã giảm giá thành công" });
                    }
                    catch (Exception ex)
                    {
                        //trả về lỗi 
                        Console.WriteLine($"Error adding apply coupon cookie: {ex.Message}");
                        return Ok(new { success = false, message = "Áp mã giảm giá thất bại" });
                    }
                }
                else
                {

                    return Ok(new { success = false, message = "Mã giảm đã hết hạn" });
                }

            }
            else
            {
                return Ok(new { success = false, message = "Mã giảm không tồn tại" });
            }

            return Json(new { CouponTitle = couponTitle });
        }

    }
}
