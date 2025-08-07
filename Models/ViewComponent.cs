using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace WEBDOAN.Models
{
    public class UserStatsViewComponent : ViewComponent
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IOnlineUserService _onlineUserService;

        public UserStatsViewComponent(UserManager<IdentityUser> userManager, IOnlineUserService onlineUserService)
        {
            _userManager = userManager;
            _onlineUserService = onlineUserService;
        }
        public class UserStatsViewModel
        {
            public int TotalUsers { get; set; }
            public int OnlineUsers { get; set; }
        }
        public IViewComponentResult Invoke()
        {
            var model = new UserStatsViewModel
            {
                TotalUsers = _userManager.Users.Count(),
                OnlineUsers = _onlineUserService.GetOnlineCount()
            };
            return View(model);
        }

       
    }

}
