using Microsoft.AspNetCore.Mvc;
using Webshopping.Models;
using Webshopping.Services.Momo;
using Webshopping.Services.ZaloPay;

namespace Webshopping.Controllers
{
	
	public class PaymentController : Controller
	{
		private IMomoService _momoService;

		public IActionResult Index()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> CreatePaymentMomo(MomoInfoModel model)
		{
			var response = await _momoService.CreatePaymentAsync(model);

			return Redirect(response.PayUrl);

		}

		[HttpGet]
		public IActionResult PaymentCallback()
		{
			var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
			return View(response);
		}
		private readonly IZaloPayService _zaloPayService;

		public PaymentController(IMomoService momoService, IZaloPayService zaloPayService)
		{
			_momoService = momoService;
			_zaloPayService = zaloPayService;
		}

		[HttpPost]
		[Route("CreateZaloPayPaymentUrl")]
		public async Task<IActionResult> CreateZaloPayPaymentUrl(ZaloInfoModel model)
		{
			var paymentUrl = await _zaloPayService.CreatePaymentUrlAsync(model);
			return Redirect(paymentUrl);
		}
		[HttpGet]
		public IActionResult PaymentCallBack()
		{
			
			var query = Request.Query;
			var apptransid = query["apptransid"];
			var status = query["status"];

			if (status == "1")
			{
				ViewBag.Message = "Thanh toán thành công qua ZaloPay!";
			}
			else
			{
				ViewBag.Message = "Thanh toán thất bại hoặc bị huỷ!";
			}

			return View(); 
		}


	}
}
