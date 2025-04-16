using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
                var shippingPriceCookie = Request.Cookies["ShippingPrice"];
                decimal shippingPrice = 0;

                if (shippingPriceCookie != null)
                {
                    var shippingPriceJson = shippingPriceCookie;
                    shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);
                }

                orderItem.ShippingCost = shippingPrice;
                orderItem.UserName = UserEmail;
                orderItem.CrateDate = DateTime.Now;
                orderItem.Status = 1;

                _dataContext.Add(orderItem);
                _dataContext.SaveChanges();

                List<CartItemModels> cartItem = HttpContext.Session.GetJson<List<CartItemModels>>("cart") ?? new List<CartItemModels>();
                foreach (var cart in cartItem)
                {
                    var orderDetail = new OrderDetail();
                    orderDetail.UserName = UserEmail;
                    orderDetail.OrderCode = ordercode;
                    orderDetail.ProductId = cart.ProductId;
                    orderDetail.Quantity = cart.Quantity;
                    orderDetail.Price = cart.Price;
                    _dataContext.Add(orderDetail);
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