using System.ComponentModel.DataAnnotations;

public class LoginViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập ")]
    public string Username { set; get; }
    [DataType(DataType.Password), Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    public string Password { set; get; }
    public string ReturnUrl { set; get; }
}