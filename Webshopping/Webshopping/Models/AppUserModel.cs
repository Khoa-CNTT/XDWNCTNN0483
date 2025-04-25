using Microsoft.AspNetCore.Identity;

namespace Webshopping.Models
{
    // Kế thừa từ IndentityUser thành ra có những properties:
    // UserName, Email, PasswordHash, Id,...
    public class AppUserModel : IdentityUser
    {
        public string Occupation { get; set; }
        public string RoleId { set; get; }
        public string Token { set; get; }
    }
}
