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
    public string Password { get; set; }
    [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu"), DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    public string ConfirmPassword { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập số điện thoại"), DataType(DataType.PhoneNumber)]
    [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ. Phải bắt đầu bằng 0 và dài 10 số.")]
    public string PhoneNumber { get; set; }
}