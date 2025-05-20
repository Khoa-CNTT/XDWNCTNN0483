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
		public MomoService(IOptions<MomoOptionModel> options)
		{
			_options = options;
		}
		public async Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(MomoInfoModel model)
{
    model.OrderId = DateTime.UtcNow.Ticks.ToString();
    model.OrderInfo = "Khách hàng: " + model.FullName + ". Nội dung: " + model.OrderInfo;
    var amountStr = model.Amount.ToString("0");  // số nguyên, không dấu phẩy

    var extraData = "email=abc@gmail.com"; // hoặc lấy từ model.ExtraData nếu có, có thể là chuỗi rỗng ""

    // Build rawData theo đúng thứ tự và trường trong mẫu của Momo
    var rawData =
    $"partnerCode={_options.Value.PartnerCode}" +
    $"&accessKey={_options.Value.AccessKey}" +
    $"&requestId={model.OrderId}" +
    $"&amount={amountStr}" +
    $"&orderId={model.OrderId}" +
    $"&orderInfo={model.OrderInfo}" +
    $"&returnUrl={_options.Value.ReturnUrl}" +
    $"&notifyUrl={_options.Value.NotifyUrl}" +
    $"&extraData={extraData}";


    Console.WriteLine("RAW DATA to sign:");
    Console.WriteLine(rawData);

    var signature = ComputeHmacSha256(rawData, _options.Value.SecretKey);

    var client = new RestClient(_options.Value.MomoApiUrl);
    var request = new RestRequest() { Method = Method.Post };
    request.AddHeader("Content-Type", "application/json; charset=UTF-8");

// Sau đó JSON gửi lên Momo
var requestData = new
{
    partnerCode = _options.Value.PartnerCode,
    accessKey = _options.Value.AccessKey,
    requestId = model.OrderId,
    amount = amountStr,
    orderId = model.OrderId,
    orderInfo = model.OrderInfo,
    returnUrl = _options.Value.ReturnUrl,
    notifyUrl = _options.Value.NotifyUrl,
    extraData = extraData,
    requestType = _options.Value.RequestType,  // Vẫn gửi nhưng không ký
    signature = signature
};

    var jsonBody = JsonConvert.SerializeObject(requestData);
    Console.WriteLine("🟡 JSON gửi đến Momo:");
    Console.WriteLine(jsonBody);

    request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);
    var response = await client.ExecuteAsync(request);
    Console.WriteLine("Response.Content từ Momo:");
    Console.WriteLine(response.Content);

    if (!response.IsSuccessful)
    {
        throw new Exception($"❌ Lỗi khi gọi Momo: {response.StatusCode} - {response.Content}");
    }

    var momoResponse = JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(response.Content);

    if (string.IsNullOrEmpty(momoResponse?.PayUrl))
    {
        throw new Exception($"❌ PayUrl null. Response: {response.Content}");
    }

    return momoResponse;
}

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