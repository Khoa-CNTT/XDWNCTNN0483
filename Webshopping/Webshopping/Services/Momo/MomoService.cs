using Microsoft.Extensions.Options;
using RestSharp;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Webshopping.Models;
using Webshopping.Models.Momo;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;

namespace Webshopping.Services.Momo
{
	public class MomoService : IMomoService
	{
		private readonly IOptions<MomoOptionModel> _options;

		// Constructor nhận đối tượng cấu hình từ DI
		public MomoService(IOptions<MomoOptionModel> options)
		{
			_options = options;
		}

		// Phương thức triển khai CreatePaymentAsync từ IMomoService
		public async Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(MomoInfoModel model)
		{
			model.OrderId = DateTime.UtcNow.Ticks.ToString();
			model.OrderInfo = "Khách hàng: " + model.FullName + ". Nội dung: " + model.OrderInfo;
			var rawData =
				$"partnerCode={_options.Value.PartnerCode}" +
				$"&accessKey={_options.Value.AccessKey}" +
				$"&requestId={model.OrderId}" +
				$"&amount={model.Amount}" +
				$"&orderId={model.OrderId}" +
				$"&orderInfo={model.OrderInfo}" +
				$"&returnUrl={_options.Value.ReturnUrl}" +
				$"&notifyUrl={_options.Value.NotifyUrl}" +
				$"&extraData=";

			var signature = ComputeHmacSha256(rawData, _options.Value.SecretKey);

			var client = new RestClient(_options.Value.MomoApiUrl);
			var request = new RestRequest() { Method = Method.Post };
			request.AddHeader("Content-Type", "application/json; charset=UTF-8");

			// Tạo đối tượng dữ liệu yêu cầu
			var requestData = new
			{
				accessKey = _options.Value.AccessKey,
				partnerCode = _options.Value.PartnerCode,
				requestType = _options.Value.RequestType,
				notifyUrl = _options.Value.NotifyUrl,
				returnUrl = _options.Value.ReturnUrl,
				orderId = model.OrderId,
				amount = model.Amount.ToString(),
				orderInfo = model.OrderInfo,
				requestId = model.OrderId,
				extraData = "",
				signature = signature
			};

			request.AddParameter("application/json", JsonConvert.SerializeObject(requestData), ParameterType.RequestBody);

			var response = await client.ExecuteAsync(request);
			var momoResponse = JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(response.Content);
			return momoResponse;
		}

		// Phương thức triển khai PaymentExecuteAsync từ IMomoService
		public MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection)
		{
			var amount = collection.First(s => s.Key == "amount").Value;
			var orderInfo = collection.First(s => s.Key == "orderInfo").Value;
			var orderId = collection.First(s => s.Key == "orderId").Value;

			return new MomoExecuteResponseModel()
			{
				Amount = amount,
				OrderId = orderId,
				OrderInfo = orderInfo
			};
		}

		// Phương thức tính toán chữ ký HMAC SHA-256
		private string ComputeHmacSha256(string message, string secretKey)
		{
			var keyBytes = Encoding.UTF8.GetBytes(secretKey);
			var messageBytes = Encoding.UTF8.GetBytes(message);

			byte[] hashBytes;

			using (var hmac = new HMACSHA256(keyBytes))
			{
				hashBytes = hmac.ComputeHash(messageBytes);
			}

			var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

			return hashString;
		}
	}
}
