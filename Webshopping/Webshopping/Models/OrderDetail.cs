using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Webshopping.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string OrderCode { get; set; }
        public int ProductId { get; set; }
        [Precision(18, 2)]
        public decimal Price { get; set; }

        public int Quantity { get; set; }
        public int Status { get; set; }

        [ForeignKey("ProductId")]
        public ProductModel Product { get; set; }
    }
}
