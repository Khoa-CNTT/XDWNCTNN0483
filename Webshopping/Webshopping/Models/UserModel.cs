namespace Webshopping.Models;

using System.ComponentModel.DataAnnotations;

public class UserModel
{
    [Key]
    public int Id { set; get; }
    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập ")]
    public string Username { set; get; }
    [Required(ErrorMessage = "Vui lòng nhập email "), EmailAddress]
    public string Email { set; get; }
    [DataType(DataType.Password), Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    public string Password { set; get; }
}