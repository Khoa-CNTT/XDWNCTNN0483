using Webshopping.Models.Vnpay;

namespace Webshopping.Services.Vnpay
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(PaymentInformationModel model, HttpContext context); // tạo url thanh toán
        PaymentResponseModel PaymentExecute(IQueryCollection collections);//  xử lý dữ liệu trả về  sau khi thanh toán

    }
}
