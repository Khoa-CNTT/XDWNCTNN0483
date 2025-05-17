namespace Webshopping.Models
{
	public class StatisticalModel
	{
		public int Id { get; set; }
		public int Quantity { get; set; }        // Số lượng tồn kho (nếu có)
		public int Sold { get; set; }            // Số lượng đã bán
		public int Revenue { get; set; }         // Doanh thu
		public int Profit { get; set; }          // Lợi nhuận
		public int Orders { get; set; }          // Tổng số đơn hàng (distinct)
		public DateTime DateCreated { get; set; } // Ngày thống kê
		public string DayOfWeekName { get; set; } // Tên thứ trong tuần (Thứ 2, Thứ 3,...)
	}
}
