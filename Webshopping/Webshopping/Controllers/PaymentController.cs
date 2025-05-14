using Microsoft.AspNetCore.Mvc;
using Webshopping.Models;
using Webshopping.Services.Momo;
using Webshopping.Services.ZaloPay;
using Webshopping.Models.Vnpay;
using Webshopping.Services.Vnpay;

namespace Webshopping.Controllers
{
	
	public class PaymentController : Controller
	{
        private readonly IMomoService _momoService;
        private readonly IZaloPayService _zaloPayService;
        private readonly IVnPayService _vnPayService;

		public PaymentController(IMomoService momoService, IZaloPayService zaloPayService, IVnPayService vnPayService)
		{
			_momoService = momoService;
			_zaloPayService = zaloPayService;
             _vnPayService = vnPayService;
		}

		public IActionResult Index()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> CreatePaymentMomo(MomoInfoModel model)
		{
			var response = await _momoService.CreatePaymentAsync(model);
			return RedirectToAction(response.PayUrl);
		}

		[HttpGet]
		public IActionResult PaymentCallback()
		{
			var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
			return View(response);
		}

		[HttpPost]
		[Route("CreateZaloPayPaymentUrl")]
		public async Task<IActionResult> CreateZaloPayPaymentUrl(ZaloInfoModel model)
		{
			var paymentUrl = await _zaloPayService.CreatePaymentUrlAsync(model);
			return RedirectToAction(paymentUrl);
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

        [HttpPost]
        public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
            return RedirectToAction(url);
        }
	}
}