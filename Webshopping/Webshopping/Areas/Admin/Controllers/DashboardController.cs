using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webshopping.Repository;
using System.Globalization;
using Webshopping.Models;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Webshopping.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Route("admin/")]
	[Route("admin/dashboard")]
	[Authorize(Roles = "Publisher,Author,Admin")]
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

		[HttpPost]
		[Route("SubmitFilterDate")]
		public IActionResult SubmitFilterDate(string filterdate)
		{
			var dateselect = DateTime.Parse(filterdate).ToString("yyyy-MM-dd");
			var chartData = _dataContext.Orders
					.Where(o => o.CreateDate.ToString("yyyy-MM-dd") == dateselect) // Optional: Filter by date
					.Join(_dataContext.OrderDetails,
								o => o.OrderCode,
								od => od.OrderCode,
								(o, od) => new StatisticalModel
								{
									DateCreated = o.CreateDate,
									Revenue = od.Quantity * (int)od.Price, // Calculate revenue based on order details
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

		[HttpPost]
		[Route("SelectFilterDate")]
		public IActionResult SelectFilterDate(string filterdate)
		{
			var chartData = new List<StatisticalModel>();
			// Initialize as empty list
			var today = DateTime.Today;
			var month = new DateTime(today.Year, today.Month, 1);
			var first = month.AddMonths(-1);
			var last = month.AddDays(-1);


			if (filterdate == "last_month")
			{
				chartData = _dataContext.Orders
					.Where(o => o.CreateDate > first && o.CreateDate < today)
					.Join(_dataContext.OrderDetails,
					o => o.OrderCode,
					od => od.OrderCode,
					(o, od) => new StatisticalModel
					{
						DateCreated = o.CreateDate,
						Revenue = od.Quantity * (int)od.Price, // Calculate revenue based on order details
															   //orders = 1 // Assuming each order detail represents one order
					})
					.GroupBy(s => s.DateCreated)
					.Select(group => new StatisticalModel
					{
						DateCreated = group.Key,
						Revenue = group.Sum(s => s.Revenue),
						//orders = group.Count()
					})
					.ToList();
			}

			return Json(chartData);
		}

		[HttpPost]
		[Route("GetChartData")]
		public IActionResult GetChartData()
		{
			var chartData = _dataContext.Orders
				.Join(_dataContext.OrderDetails,
					o => o.OrderCode,
					od => od.OrderCode,
					(o, od) => new StatisticalModel
					{
						DateCreated = o.CreateDate,
						Revenue = od.Quantity * (int)od.Price, // Calculate revenue based on order details
															   //orders = 1 // Assuming each order detail represents one order
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
	}
}