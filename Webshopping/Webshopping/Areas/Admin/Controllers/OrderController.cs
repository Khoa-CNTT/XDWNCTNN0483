using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webshopping.Models;
using Webshopping.Repository;

namespace Webshopping.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/order/")]
    public class OrderController : Controller
    {
        private readonly DataContext _dataContext;
        public OrderController(DataContext context)
        {
            _dataContext = context;
        }

        // GET: admin/order/
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var orderDetails = await _dataContext.OrderDetails
                            .Include(od => od.Product)
                            .ToListAsync();

            // Nếu bạn cần thêm thông tin như shipping cost hay status, gán ViewBag ở đây
            ViewBag.ShippingCost = 20000;
            ViewBag.Status = 1;

            return View(orderDetails);
        }

        [HttpGet("view/{ordercode}")]
        public async Task<IActionResult> ViewOrder(string ordercode)
        {
            if (string.IsNullOrEmpty(ordercode))
            {
                return BadRequest("Mã đơn hàng không hợp lệ.");
            }

            var DetailsOrder = await _dataContext.OrderDetails
                .Include(od => od.Product)
                .Where(od => od.OrderCode == ordercode)
                .ToListAsync();

            var Order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);

            if (Order == null)
            {
                return NotFound("Không tìm thấy đơn hàng.");
            }

            ViewBag.ShippingCost = Order.ShippingCost;
            ViewBag.Status = Order.Status;

            return View(DetailsOrder);
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
