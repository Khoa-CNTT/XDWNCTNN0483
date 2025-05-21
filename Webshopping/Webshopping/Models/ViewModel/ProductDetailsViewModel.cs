using System.ComponentModel.DataAnnotations;

namespace Webshopping.Models.ViewModel
{
    public class ProductDetailsViewModel
    {
        public ProductModel ProductDetails { get; set; }
        public RatingModel RatingDetails { get; set; }
        [Required]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Comment { get; set; }

        public string Star { get; set; }
        public string Infomation { get; set; }
    }
}
