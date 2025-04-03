using Microsoft.AspNetCore.Identity;

namespace Webshopping.Models
{
    public class AppUserModel: IdentityUser
    {
        public string Occupation { get; set; }
    }
}
