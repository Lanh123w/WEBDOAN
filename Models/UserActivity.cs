using Microsoft.AspNetCore.Identity;

namespace WEBDOAN.Models
{
    public class UserActivity
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime LastLoginTime { get; set; }
        
        public IdentityUser User { get; set; }
    }


}
