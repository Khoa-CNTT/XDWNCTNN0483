namespace Webshopping.Models
{
    public class OrderModel
    {
        public int Id { get; set; }
        public string OrderCode { get; set; }
        public string UserName { get; set; }
        public DateTime CrateDate { get; set; }
        public int Status { get; set; }
    }
}
