public class UserViewModel
{
    public string Id { get; set; } // User ID
    public string UserName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public List<string> RoleIds { get; set; } // Selected Role IDs
}