namespace Webshopping.Areas.Admin.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webshopping.Models;
using Webshopping.Repository;

[Area("Admin")]
[Route("admin/shipping/")]
[Authorize(Roles = "employee,Admin")]
public class ShippingController : Controller
{
    private readonly DataContext _dataContext;

    // Constructor
    public ShippingController(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    // GET: admin/shipping/
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var shippingList = await _dataContext.Shippings.ToListAsync();
        ViewBag.Shippings = shippingList;
        return View();
    }

    // POST: admin/shipping/store-shipping
    [HttpPost("store-shipping")]
    public async Task<IActionResult> StoreShipping(ShippingModel shippingModel, string phuong, string quan, string tinh, decimal price)
    {
        shippingModel.City = tinh;
        shippingModel.Distric = quan;
        shippingModel.Ward = phuong;
        shippingModel.Price = price;

        try
        {
            var exists = await _dataContext.Shippings
                .AnyAsync(x => x.City == tinh && x.Distric == quan && x.Ward == phuong);

            if (exists)
                return Ok(new { duplicate = true });

            _dataContext.Shippings.Add(shippingModel);
            await _dataContext.SaveChangesAsync();

            return Ok(new { success = true, id = shippingModel.Id }); // Gửi id về client
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred.");
        }
    }

    // POST: admin/shipping/{id}
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var shipping = await _dataContext.Shippings.FindAsync(id);
        if (shipping == null)
        {
            TempData["error"] = "Không tìm thấy bản ghi cần xóa.";
            return RedirectToAction("Index");
        }

        _dataContext.Shippings.Remove(shipping);
        await _dataContext.SaveChangesAsync();
        TempData["success"] = "Shipping đã được xóa thành công";
        return RedirectToAction("Index");
    }
}
