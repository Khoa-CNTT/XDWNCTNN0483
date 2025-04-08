namespace Webshopping.Models.ViewModel;

using System.ComponentModel.DataAnnotations;

public class UserViewModel
{
    public string Id { get; set; }
    [Required(ErrorMessage = "Yêu cầu nhập tên đăng nhập")]
    public string UserName { get; set; }
    [Required(ErrorMessage = "Yêu cầu nhập email")]
    [EmailAddress]
    public string Email { get; set; }
    [Required(ErrorMessage = "Yêu cầu nhập số điện thoại")]
    public string PhoneNumber { get; set; }
    [Required(ErrorMessage = "Yêu cầu nhập mật khẩu")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
    public string RoleId { get; set; }
}