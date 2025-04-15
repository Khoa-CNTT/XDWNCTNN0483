namespace Webshopping.Models;

using System.ComponentModel.DataAnnotations;

public class ShippingModel
{
    [Key]
    public int Id { set; get; }
    public decimal Price { set; get; }
    public string Ward { set; get; }
    public string Distric { set; get; }
    public string City { set; get; }
    // public DateTime DateCreated { set; get; }
}