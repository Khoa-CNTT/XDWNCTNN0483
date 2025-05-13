namespace Webshopping.Models;

using global::System.ComponentModel.DataAnnotations;

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
    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [DataType(DataType.PhoneNumber)]
    [RegularExpression(@"^(84|0[3|5|7|8|9])[0-9]{8}\b", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string PhoneNumber { get; set; }
}