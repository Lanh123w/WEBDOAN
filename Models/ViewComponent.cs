using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WEBDOAN.Models;
using WEBDOAN.Models;
using WEBDOAN.Data;

namespace WEBDOAN.ViewComponents
{
    public class UserStatsViewComponent : ViewComponent
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IOnlineUserService _onlineUserService;
        private readonly ApplicationDbContext _context;

        public UserStatsViewComponent(
            UserManager<IdentityUser> userManager,
            IOnlineUserService onlineUserService,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _onlineUserService = onlineUserService;
            _context = context;
        }

        public class UserStatsViewModel
        {
            public int TotalUsers { get; set; }
            public int OnlineUsers { get; set; }
            public int TodayVisitCount { get; set; }
            public int TotalVisitCount { get; set; }
        }

        public IViewComponentResult Invoke()
        {
            var today = DateTime.Today;

            var model = new UserStatsViewModel
            {
                TotalUsers = _userManager.Users.Count(),
                OnlineUsers = _onlineUserService.GetOnlineCount(),
                TodayVisitCount = _context.VisitLogs.Count(v => v.VisitDate.Date == today),
                TotalVisitCount = _context.VisitLogs.Count()
            };

            return View(model);
        }
    }
}
