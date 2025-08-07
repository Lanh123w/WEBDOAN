using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Data;
using WebApplication3.Models;
using WEBDOAN.Models;

[Authorize(Roles = "Admin")]
public class AdminOrderController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public AdminOrderController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // 📦 Hiển thị danh sách đơn hàng với bộ lọc trạng thái
    public async Task<IActionResult> Index(string statusFilter)
    {
        var query = _context.Orders
            .Include(o => o.OrderDetails)
            .ThenInclude(d => d.FoodItem)
            .AsQueryable();

        if (!string.IsNullOrEmpty(statusFilter))
        {
            query = query.Where(o => o.Status == statusFilter);
        }

        var orders = await query
            .Select(o => new AdminOrderViewModel
            {
                Id = o.Id,
                CustomerName = o.CustomerName ?? "Không có tên",
                Address = o.Address ?? "Chưa có địa chỉ",
                Phone = o.Phone ?? "Chưa có số",
                OrderDate = o.OrderDate,
                Status = o.Status ?? "Chưa xác định",
                TotalAmount = o.TotalAmount,
                ItemTotal = o.OrderDetails.Sum(d => d.Quantity * d.UnitPrice),
                DiscountCode = o.DiscountCode,
                OrderDetails = o.OrderDetails.ToList()
            })
            .ToListAsync();

        ViewBag.CurrentFilter = statusFilter;
        return View(orders);
    }

    // 🔄 Cập nhật trạng thái đơn hàng
    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            TempData["ToastMessage"] = $"❌ Không tìm thấy đơn hàng #{id}.";
            return RedirectToAction(nameof(Index));
        }

        // ✅ Ngăn admin cập nhật nếu người dùng đã xác nhận
        if (order.Status == "Đã nhận hàng")
        {
            TempData["ToastMessage"] = $"⚠️ Đơn hàng #{id} đã được người dùng xác nhận. Không thể cập nhật.";
            return RedirectToAction(nameof(Index));
        }

        order.Status = status;
        await _context.SaveChangesAsync();

        TempData["ToastMessage"] = $"✅ Đã cập nhật trạng thái đơn hàng #{id} thành '{status}'.";
        return RedirectToAction(nameof(Index));
    }

    // 📋 Danh sách người dùng
    public IActionResult ListUser()
    {
        var users = _userManager.Users.ToList();
        return View(users);
    }

    // 🔍 Chi tiết người dùng
    public async Task<IActionResult> Details(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["ToastMessage"] = $"❌ Không tìm thấy người dùng.";
            return RedirectToAction(nameof(Index));
        }

        var roles = await _userManager.GetRolesAsync(user);
        ViewBag.Roles = roles;
        return View(user);
    }

    // ✅ Gán vai trò
    [HttpPost]
    public async Task<IActionResult> AssignRole(string id, string role)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["ToastMessage"] = $"❌ Không tìm thấy người dùng.";
            return RedirectToAction(nameof(Index));
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, role);

        TempData["ToastMessage"] = $"✅ Đã gán vai trò '{role}' cho {user.UserName}.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // 🔒 Khoá / Mở khoá tài khoản
    [HttpPost]
    public async Task<IActionResult> ToggleLock(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["ToastMessage"] = $"❌ Không tìm thấy người dùng.";
            return RedirectToAction(nameof(Index));
        }

        if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
        {
            user.LockoutEnd = null;
            TempData["ToastMessage"] = $"🔓 Đã mở khoá tài khoản {user.UserName}.";
        }
        else
        {
            user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
            TempData["ToastMessage"] = $"🔒 Đã khoá tài khoản {user.UserName}.";
        }

        await _userManager.UpdateAsync(user);
        return RedirectToAction(nameof(Details), new { id });
    }

    // 🗑️ Xoá người dùng
    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            await _userManager.DeleteAsync(user);
            TempData["ToastMessage"] = $"🗑️ Đã xoá người dùng {user.UserName}.";
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Stats()
{
  
    var users = _userManager.Users.ToList();
    var totalUsers = users.Count;
    var lockedUsers = users.Count(u => u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow);

    int admins = 0;
    foreach (var user in users)
    {
        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            admins++;
        }
    }

    
    var today = DateTime.Today;
    var weekStart = today.AddDays(-(int)today.DayOfWeek);
    var monthStart = new DateTime(today.Year, today.Month, 1);

    var totalToday = _context.Orders.Count(o => o.OrderDate.Date == today);
    var totalWeek = _context.Orders.Count(o => o.OrderDate >= weekStart);
    var totalMonth = _context.Orders.Count(o => o.OrderDate >= monthStart);

    var revenueToday = _context.OrderDetails
        .Where(d => d.Order.OrderDate.Date == today)
        .Sum(d => d.Quantity * d.UnitPrice);

    var revenueMonth = _context.OrderDetails
        .Where(d => d.Order.OrderDate >= monthStart)
        .Sum(d => d.Quantity * d.UnitPrice);

        // 🔥 Món bán chạy nhất (Top 5)
    var topFoods = _context.OrderDetails
        .GroupBy(d => d.FoodItem.Name)
        .Select(g => new
        {
            Food = g.Key,
            Quantity = g.Sum(x => x.Quantity)
        })
        .OrderByDescending(x => x.Quantity)
        .Take(5)
        .ToList();

    
    var topUsers = _context.Orders
        .GroupBy(o => o.CustomerName)
        .Select(g => new
        {
            User = g.Key,
            Orders = g.Count()
        })
        .OrderByDescending(x => x.Orders)
        .Take(5)
        .ToList();

   
    var stats = new
    {
        TotalUsers = totalUsers,
        LockedUsers = lockedUsers,
        Admins = admins,
        TotalOrdersToday = totalToday,
        TotalOrdersWeek = totalWeek,
        TotalOrdersMonth = totalMonth,
        RevenueToday = revenueToday,
        RevenueMonth = revenueMonth,
        TopFoods = topFoods,
        TopUsers = topUsers
    };

    return View(stats);
}
}

