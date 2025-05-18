using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webshopping.Repository;
using System.Globalization;
using Webshopping.Models;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Webshopping.Areas.Admin.Controllers
{
	public record ChartRequest(
		DateTime StartDate,
		DateTime EndDate,
		string FilterType
	);

	[Area("Admin")]
	[Route("admin/")]
	[Route("admin/dashboard")]
	[Authorize(Roles = "Employee,Admin")]
	public class DashboardController : Controller
	{
		private const int v = 2025;
		private readonly DataContext _dataContext;

		public DashboardController(DataContext context)
		{
			_dataContext = context;
		}

		[HttpGet("")]
		public IActionResult Index()
		{
			var count_product = _dataContext.Products.Count();
			var count_order = _dataContext.Orders.Count();
			var count_category = _dataContext.Categories.Count();
			var count_user = _dataContext.Users.Count();
			ViewBag.CountProduct = count_product;
			ViewBag.CountOrder = count_order;
			ViewBag.CountCategory = count_category;
			ViewBag.CountUser = count_user;
			return View();
		}

		[HttpPost("submit-filter-date")]
		public IActionResult SubmitFilterDate(string fromDate, string toDate)
		{
			// Parse từ string sang DateTime, bạn có thể thêm xử lý TryParse nếu muốn
			DateTime from = DateTime.Parse(fromDate);
			DateTime to = DateTime.Parse(toDate);

			// Lọc đơn hàng trong khoảng từ ngày 'from' đến 'to' (bao gồm cả hai đầu)
			var chartData = _dataContext.Orders
				.Where(o => o.CreateDate.Date >= from.Date && o.CreateDate.Date <= to.Date)
				.Join(_dataContext.OrderDetails,
						o => o.OrderCode,
						od => od.OrderCode,
						(o, od) => new StatisticalModel
						{
							DateCreated = o.CreateDate.Date,
							Revenue = od.Quantity * (int)od.Price,
						})
				.GroupBy(s => s.DateCreated)
				.Select(group => new StatisticalModel
				{
					DateCreated = group.Key,
					Revenue = group.Sum(s => s.Revenue),
					//orders = group.Count()
				})
				.ToList();

			return Json(chartData);
		}

		[HttpPost("select-filter-date")]
		public IActionResult SelectFilterDate(string filterdate)
		{
			var chartData = new List<StatisticalModel>();
			var today = DateTime.Today;
			var firstDayOfCurrentMonth = new DateTime(today.Year, today.Month, 1);
			var firstDayOfLastMonth = firstDayOfCurrentMonth.AddMonths(-1);
			var lastDayOfLastMonth = firstDayOfCurrentMonth.AddDays(-1);

			if (filterdate == "last_month")
			{
				chartData = _dataContext.Orders
					.Where(o => o.CreateDate.Date >= firstDayOfLastMonth && o.CreateDate.Date <= lastDayOfLastMonth)
					.Join(_dataContext.OrderDetails,
						o => o.OrderCode,
						od => od.OrderCode,
						(o, od) => new StatisticalModel
						{
							DateCreated = o.CreateDate.Date,
							Revenue = od.Quantity * (int)od.Price,
							// Thêm thuộc tính này nếu có trong model StatisticalModel, hoặc tạo thêm thuộc tính DayOfWeekName
							DayOfWeekName = GetVietnameseDayOfWeek(o.CreateDate.DayOfWeek)
						})
					.GroupBy(s => s.DateCreated)
					.Select(group => new StatisticalModel
					{
						DateCreated = group.Key,
						Revenue = group.Sum(s => s.Revenue),
						DayOfWeekName = GetVietnameseDayOfWeek(group.Key.DayOfWeek)
					})
					.OrderBy(s => s.DateCreated)
					.ToList();
			}

			return Json(chartData);
		}

		[HttpPost("get-chart-data")]
		public IActionResult GetChartData([FromBody] ChartRequest req)
		{
			// Bảo đảm EndDate bao trùm hết ngày kết thúc
			var startDate = req.StartDate.Date;
			var endDate = req.EndDate.Date.AddDays(1); // so sánh < endDate

			var orderDetails = _dataContext.Orders
				.Where(o => o.CreateDate >= startDate && o.CreateDate < endDate)
				.Join(_dataContext.OrderDetails,
					o => o.OrderCode,
					od => od.OrderCode,
					(o, od) => new
					{
						o.Id,
						o.CreateDate,
						od.Quantity,
						od.Price,
						od.ProductId,
						o.OrderCode
					})
				.Join(_dataContext.Products,
					od => od.ProductId,
					p => p.Id,
					(od, p) => new
					{
						od.Id,
						Date = od.CreateDate,
						Revenue = od.Quantity * (int)od.Price,
						Profit = od.Quantity * ((int)od.Price - (int)p.CapitalPrice),
						Quantity = od.Quantity,
						od.OrderCode
					});

			var groupedData = orderDetails
				.AsEnumerable()
				.GroupBy(item =>
				{
					return req.FilterType?.ToLower() switch
					{
						"day" => item.Date.Date,
						"week" =>
							item.Date.AddDays(-(int)(item.Date.DayOfWeek == 0
										? 6
										: item.Date.DayOfWeek - DayOfWeek.Monday)).Date,
						"month" => new DateTime(item.Date.Year, item.Date.Month, 1),
						"year" => new DateTime(item.Date.Year, 1, 1),
						_ => item.Date.Date
					};
				})
				.Select(g => new StatisticalModel
				{
					DateCreated = g.Key,
					Revenue = g.Sum(x => x.Revenue),
					Profit = g.Sum(x => x.Profit),
					Sold = g.Sum(x => x.Quantity),
					Orders = g.Select(x => x.Id).Distinct().Count()
				})
				.OrderBy(g => g.DateCreated)
				.ToList();

			return Json(groupedData);
		}

		[HttpPost("get-user-chart-data")]
		public JsonResult GetUserChartData()
		{
			var chartData = _dataContext.Users
				.GroupBy(u => u.CreatedDate.Date)
				.Select(g => new
				{
					date = g.Key.ToString("yyyy-MM-dd"),
					count = g.Count()
				}).ToList();

			return Json(chartData); // không bọc thêm object nào
		}

		// Hàm chuyển DayOfWeek sang tên tiếng Việt
		private string GetVietnameseDayOfWeek(DayOfWeek dayOfWeek)
		{
			return dayOfWeek switch
			{
				DayOfWeek.Monday => "Thứ 2",
				DayOfWeek.Tuesday => "Thứ 3",
				DayOfWeek.Wednesday => "Thứ 4",
				DayOfWeek.Thursday => "Thứ 5",
				DayOfWeek.Friday => "Thứ 6",
				DayOfWeek.Saturday => "Thứ 7",
				DayOfWeek.Sunday => "Chủ nhật",
				_ => ""
			};
		}

		[AllowAnonymous]
		[HttpGet("products-combined-chart-data")]
		public async Task<IActionResult> ProductsCombinedChart()
		{
			// Đếm tổng số bản ghi
			var totalProducts = await _dataContext.Products.CountAsync();
			var totalBrands = await _dataContext.Brands.CountAsync();
			var totalCategories = await _dataContext.Categories.CountAsync();

			// Trả về 1 mảng 3 phần tử
			var combined = new[]
			{
				new { label = "Sản phẩm",       count = totalProducts },
				new { label = "Nhãn hiệu",      count = totalBrands },
				new { label = "Loại hàng",      count = totalCategories }
			};

			return Json(combined);
		}
	}
}