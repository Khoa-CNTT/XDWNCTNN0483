namespace Webshopping.Models.ViewModel;

using System.ComponentModel.DataAnnotations;

public class UserViewModel : IValidatableObject
{
    public string Id { get; set; }
    [Required(ErrorMessage = "Yêu cầu nhập tên đăng nhập")]
    public string UserName { get; set; }
    [Required(ErrorMessage = "Yêu cầu nhập email")]
    [EmailAddress]
    public string Email { get; set; }
    [Required(ErrorMessage = "Yêu cầu nhập số điện thoại")]
    public string PhoneNumber { get; set; }
    [DataType(DataType.Password)]
    public string Password { get; set; }
    public string RoleId { get; set; }
    // Cho phép bỏ qua mật khẩu khi edit
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var isCreate = string.IsNullOrEmpty(Id); // Nếu không có Id tức là đang tạo mới

        if (isCreate && string.IsNullOrWhiteSpace(Password))
        {
            yield return new ValidationResult("Yêu cầu nhập mật khẩu", new[] { nameof(Password) });
        }
    }
}
