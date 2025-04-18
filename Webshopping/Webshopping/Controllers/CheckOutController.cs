using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Webshopping.Models;
using Webshopping.Repository;
using Webshopping.Repository.Shopping_Tutorial.Repository;

namespace Webshopping.Controllers
{
    public class CheckOutController : Controller
    {
        private readonly DataContext _dataContext;
        public CheckOutController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }
        public async Task<IActionResult> Checkout()
        {
            var UserEmail = User.FindFirstValue(ClaimTypes.Email);
            if (UserEmail == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                var ordercode = Guid.NewGuid().ToString();
                var orderItem = new OrderModel();
                orderItem.OrderCode = ordercode;
                // nhận shipping
                var shippingPriceCookie = Request.Cookies["ShippingPrice"];
                decimal shippingPrice = 0;
                //Nhận Coupon code từ cookie
                var coupon_code = Request.Cookies["CouponTitle"];
                if (shippingPriceCookie != null)
                {
                    var shippingPriceJson = shippingPriceCookie;
                    shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);
                }

                orderItem.ShippingCost = shippingPrice;
                orderItem.CouponCode = coupon_code;
                orderItem.UserName = UserEmail;
                orderItem.CrateDate = DateTime.Now;
                orderItem.Status = 1;

                _dataContext.Add(orderItem);
                _dataContext.SaveChanges();

                //Tạo đơn hàng
                List<CartItemModels> cartItem = HttpContext.Session.GetJson<List<CartItemModels>>("cart") ?? new List<CartItemModels>();
                foreach (var cart in cartItem)
                {
                    var orderDetail = new OrderDetail(); // Corrected variable name
                    orderDetail.UserName = UserEmail;
                    orderDetail.OrderCode = ordercode;
                    orderDetail.ProductId = cart.ProductId; // Corrected variable name
                    orderDetail.Price = cart.Price; // Corrected variable name
                    orderDetail.Quantity = cart.Quantity; // Corrected variable name
                                                          //update product quantity
                    var product = await _dataContext.Products.Where(p => p.Id == cart.ProductId).FirstAsync();
                    product.Quantity -= cart.Quantity;
                    product.Sold += cart.Quantity;
                    _dataContext.Update(product);
                    _dataContext.Add(orderDetail); // Corrected variable name
                    _dataContext.SaveChanges();

                }
                HttpContext.Session.Remove("cart");
                TempData["success"] = "Checkout thành công,vui lòng đợi duyệt đơn hàng";
                return RedirectToAction("Index", "Cart");

            }
            return View();
        }
    }
}