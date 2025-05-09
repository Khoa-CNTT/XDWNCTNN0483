using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System.Security.Cryptography;
using System.Text;
using Webshopping.Models;
using Webshopping.Models.ZaloPay;

namespace Webshopping.Services.ZaloPay
{
	public class ZaloPayService : IZaloPayService
	{
		private readonly IOptions<ZaloPayOptionModel> _options;

		public ZaloPayService(IOptions<ZaloPayOptionModel> options)
		{
			_options = options;
		}

		public async Task<string> CreatePaymentUrlAsync(ZaloInfoModel model)
		{
			var appid = _options.Value.AppId;
			var key1 = _options.Value.Key1;
			var endpoint = _options.Value.Endpoint;

			var transId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			var embeddata = "{}";
			var items = "[]";

			var order = new Dictionary<string, string>
		{
			{ "appid", appid },
			{ "appuser", model.FullName },
			{ "apptime", transId.ToString() },
			{ "amount", model.Amount.ToString() },
			{ "apptransid", DateTime.Now.ToString("yyMMdd") + "_" + new Random().Next(100000,999999).ToString() }, // Ví dụ 240427_123456
            { "embeddata", embeddata },
			{ "item", items },
			{ "description", "Thanh toán đơn hàng tại Website" },
			{ "bankcode", "zalopayapp" }, // Bank code nếu cần chọn ngân hàng cụ thể
            { "callbackurl", _options.Value.CallbackUrl }
		};

			var data = $"{order["appid"]}|{order["apptransid"]}|{order["appuser"]}|{order["amount"]}|{order["apptime"]}|{order["embeddata"]}|{order["item"]}";
			order["mac"] = HmacSHA256(key1, data);

			var client = new RestClient(endpoint);
			var request = new RestRequest() { Method = Method.Post };
			request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

			foreach (var kv in order)
			{
				request.AddParameter(kv.Key, kv.Value);
			}

			var response = await client.ExecuteAsync(request);

			var json = JsonConvert.DeserializeObject<dynamic>(response.Content);
			return json.orderurl;
		}

		private string HmacSHA256(string key, string data)
		{
			using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
			{
				var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
				return BitConverter.ToString(hash).Replace("-", "").ToLower();
			}
		}
	}
}
