using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Webshopping.Models;
using Webshopping.Repository;
using Webshopping.Repository.Webshopping.Repository;
using Microsoft.AspNetCore.Identity;
using Webshopping.Areas.Admin.Repository;
using Webshopping.Services.Vnpay;
using System.Threading.Tasks;
using PayPal.Api;

namespace Webshopping.Controllers
{
    public class CheckOutController : Controller
    {
        private readonly IVnPayService _vnPayService;
        private readonly DataContext _dataContext;
        private readonly IEmailSender _emailSender;
        public CheckOutController(IEmailSender emailSender, DataContext dataContext, IVnPayService vnPayService)
        {
            _dataContext = dataContext;
            _emailSender = emailSender;
            _vnPayService = vnPayService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Checkout(string PaymentMethod, string PaymentId)
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
                else
                {
                    shippingPrice = 0;
                }
                orderItem.ShippingCost = shippingPrice;
                orderItem.CouponCode = coupon_code;
                orderItem.UserName = UserEmail;
                orderItem.PaymentMethod = PaymentMethod + " " + PaymentId;
                orderItem.CreateDate = DateTime.Now;
                orderItem.Status = 1;
                _dataContext.Add(orderItem);
                _dataContext.SaveChanges();
                //Tạo đơn hàng
                List<CartItemModels> cartItem = HttpContext.Session.GetJson<List<CartItemModels>>("Cart") ?? new List<CartItemModels>();
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
                    await _dataContext.SaveChangesAsync();
                }
                HttpContext.Session.Remove("Cart");
                //Gui mail cho người dùng   
                TempData["success"] = "Checkout thành công,vui lòng đợi duyệt đơn hàng";
                return RedirectToAction("History", "Account");
            }
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);
            if (response.VnPayResponseCode == "00") //Giao dịch thành công luu vào db
            {
                var newVnpayInsert = new VnpayModel
                {
                    OrderId = response.OrderId,
                    PaymentMethod = response.PaymentMethod,
                    OrderDescription = response.OrderDescription,
                    TransactionId = response.TransactionId,
                    PaymentId = response.PaymentId,
                    DateCreated = DateTime.Now
                };
                _dataContext.Add(newVnpayInsert);
                await _dataContext.SaveChangesAsync();
                //Tiến hành đặt đơn hàng khi thanh toán vnpay thành công
                var PaymentMethod = response.PaymentMethod;
                var PaymentId = response.PaymentId;
                await Checkout(PaymentMethod, PaymentId);
            }
            else
            {
                TempData["success"] = "Giao dịch thành công";
                return RedirectToAction("Index", "Cart");
            }
            //return Json(response);
            return View(response);
        }
    }
}