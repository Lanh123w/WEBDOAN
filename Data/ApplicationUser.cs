using Microsoft.AspNetCore.Identity;

public class ApplicationUser
{
    public string UserName { get; set; }
    public string Email { get; set; }
    public bool EmailConfirmed { get; set; }
    public DateTime? LastLoginTime { get; set; }


}