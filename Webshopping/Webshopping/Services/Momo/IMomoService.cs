using Webshopping.Models;
using Webshopping.Models.Momo;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Webshopping.Services.Momo
{
	public interface IMomoService
	{
		Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(MomoInfoModel model);
		MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
	}
}
