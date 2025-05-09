using Webshopping.Models;

namespace Webshopping.Services.ZaloPay
{
	public interface IZaloPayService
	{
		Task<string> CreatePaymentUrlAsync(ZaloInfoModel model); // phải có dấu ;
	}

}
