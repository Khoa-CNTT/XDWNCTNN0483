using Microsoft.AspNetCore.Mvc;
using Webshopping.Models;
using Webshopping.Services.Momo;
using Webshopping.Models.Momo;
using Webshopping.Models.Vnpay;
using Webshopping.Services.Vnpay;
using Newtonsoft.Json;

namespace Webshopping.Controllers
{
	
	public class PaymentController : Controller
	{
        private readonly IMomoService _momoService;
        private readonly IVnPayService _vnPayService;

		public PaymentController(IMomoService momoService, IVnPayService vnPayService)
		{
			_momoService = momoService;
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
			if (response == null || string.IsNullOrEmpty(response.PayUrl))
{
    // Ghi log nếu cần
    System.IO.File.AppendAllText("Logs/momo-response-log.txt", $"[{DateTime.Now}] ❌ PayUrl is null. Full response: {JsonConvert.SerializeObject(response)}\n");

    // Hiển thị thông báo lỗi hoặc redirect về trang thông báo
    return BadRequest("Không thể tạo liên kết thanh toán Momo. Vui lòng thử lại sau.");
}

return Redirect(response.PayUrl);

		}


		[HttpGet]
		public IActionResult PaymentCallBack()
		{
			var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
			return View(response);
		}

        [HttpPost]
        public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
            return Redirect(url);
        }
	}
}