namespace Webshopping.Models.ViewModel
{
	public class CartItemViewModel
	{
		public List<CartItemModels> CartItems { get; set; }
		public decimal GrandTotal { get; set; }
		public decimal ShippingPrice { get; set; } // Đây là thuộc tính bạn đang hỏi
		public string CouponCode {  get; set; }
	}
}
