using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using WebApplication3.Data;
using WebApplication3.Models;
using WEBDOAN.Models;
namespace WebApplication3.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IOnlineUserService _onlineUserService;



        public HomeController(
       ILogger<HomeController> logger,
       ApplicationDbContext context,
       UserManager<IdentityUser> userManager,
       IOnlineUserService onlineUserService)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _onlineUserService = onlineUserService;
        }

        public async Task<IActionResult> Index()
        {
            var foodItems = await _context.FoodItems.ToListAsync();
            var random = new Random();
            var suggestedItem = foodItems.Count > 0 ? foodItems[random.Next(foodItems.Count)] : null;

            ViewData["SuggestedItem"] = suggestedItem;

            var topFoodItems = _context.OrderDetails
                .GroupBy(od => od.FoodItemId)
                .Select(g => new {
                    FoodItemId = g.Key,
                    TotalOrdered = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.TotalOrdered)
                .Take(3)
                .Join(_context.FoodItems,
                     o => o.FoodItemId,
                     f => f.Id,
                     (o, f) => f)
                .ToList();

            ViewData["TopFoodItems"] = topFoodItems;

            // 👥 Tổng số người đã đăng ký
            ViewBag.TotalUsers = _userManager.Users.Count();

            // 🟢 Đang online
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _onlineUserService.UserActive(userId);
            }
            ViewBag.OnlineCount = _onlineUserService.GetOnlineCount();

            return View();
        }
        public IActionResult OnlineStatsPartial()
        {
            return ViewComponent("UserStats");
        }

        public IActionResult MapTest()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
