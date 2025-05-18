using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webshopping.Models;
using Webshopping.Repository;

namespace Webshopping.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Route("admin/order/")]
	[Authorize(Roles = "Employee,Admin")]
	public class OrderController : Controller
	{
		private readonly DataContext _dataContext;

		public OrderController(DataContext context)
		{
			_dataContext = context;
		}

		[HttpGet("")]
		public async Task<IActionResult> Index()
		{
			return View(await _dataContext.Orders.OrderByDescending(p => p.Id).ToListAsync());
		}

		[HttpGet("view")]
		public async Task<IActionResult> ViewOrder(string ordercode)
		{
			var DetailsOrder = await _dataContext.OrderDetails.Include(od => od.Product)
				.Where(od => od.OrderCode == ordercode).ToListAsync();

			var Order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);

			if (Order == null)
			{
				return NotFound("Không tìm thấy đơn hàng.");
			}

			ViewBag.CouponCode = Order.CouponCode;

			if (!string.IsNullOrEmpty(Order.CouponCode))
			{
				// Tách mã coupon trước khi so sánh
				var couponCode = Order.CouponCode?.Split('|')[0]?.Trim();

				var coupon = await _dataContext.Coupons
					.FirstOrDefaultAsync(c => c.Name == couponCode);

				if (coupon != null)
				{
					ViewBag.DiscountType = coupon.DiscountType; // 0 = phần trăm, 1 = số tiền
					ViewBag.DiscountValue = coupon.DiscountValue;
				}
				else
				{
					ViewBag.DiscountType = null;
					ViewBag.DiscountValue = null;
				}
			}
			else
			{
				ViewBag.DiscountType = null;
				ViewBag.DiscountValue = null;
			}

			ViewBag.ShippingCost = Order.ShippingCost;
			ViewBag.Status = Order.Status;

			return View(DetailsOrder);
		}

		[HttpGet("PaymentVnpayInfo")]
		public async Task<IActionResult> PaymentVnpayInfo(string orderId)
		{
			var vnPayInfo = await _dataContext.VnInfos.FirstOrDefaultAsync(m => m.PaymentId == orderId);
			if (vnPayInfo == null)
			{
				return NotFound();
			}
			return View(vnPayInfo);
		}

		[HttpPost]
		[Route("UpdateOrder")]
		public async Task<IActionResult> UpdateOrder(string ordercode, int status)
		{
			var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);

			if (order == null)
			{
				return NotFound();
			}

			order.Status = status;

			try
			{
				await _dataContext.SaveChangesAsync();
				return Ok(new { success = true, message = "Order status updated successfully" });
			}
			catch (Exception)
			{
				return StatusCode(500, "An error occurred while updating the order status.");
			}
		}

		[HttpGet]
		[Route("Delete")]
		public async Task<IActionResult> Delete(string ordercode)
		{
			var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);

			if (order == null)
			{
				return NotFound();
			}
			try
			{
				//delete order
				_dataContext.Orders.Remove(order);
				await _dataContext.SaveChangesAsync();

				return RedirectToAction("Index");
			}
			catch (Exception)
			{
				return StatusCode(500, "An error occurred while deleting the order.");
			}
		}

	}
}