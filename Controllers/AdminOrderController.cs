using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using WebApplication3.Data;
using WebApplication3.Models;
using WEBDOAN.Models;

[Authorize(Roles = "Admin")]
public class AdminOrderController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IMemoryCache _cache;


    public AdminOrderController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IMemoryCache cache)
    {
        _context = context;
        _userManager = userManager;
        _cache = cache;
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

    public IActionResult ListUser()
{
    // 📦 Lấy danh sách người dùng từ Identity
    var users = _userManager.Users.ToList();

    // 🔍 Lấy danh sách người dùng đang online từ cache
    var onlineUsers = _cache.Get<Dictionary<string, DateTime>>("OnlineUsers") ?? new();

    // 📊 Lấy lịch sử đăng nhập từ bảng UserActivity
    var activityLogs = _context.UserActivities
        .Join(_context.Users,
              a => a.UserId,
              u => u.Id,
              (a, u) => new ActivityLogViewModel
              {
                  UserId = u.Id,
                  UserName = u.UserName,
                  LastLoginTime = a.LastLoginTime
              })
        .OrderByDescending(x => x.LastLoginTime)
        .ToList();

    // 🧠 Gắn dữ liệu vào ViewBag để truyền sang View
    ViewBag.OnlineUsers = onlineUsers;
    ViewBag.ActivityLogs = activityLogs;

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

    // 📊 Thống kê người dùng
    public async Task<IActionResult> Stats()
    {
        // 📌 Thống kê người dùng
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

        // 📌 Thống kê đơn hàng & doanh thu
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

        // 👥 Người dùng hoạt động nhiều nhất (Top 5)
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

        // 📦 Gộp tất cả vào model động
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

public async Task<IActionResult> ToggleStatus(int id)
{
    var food = await _context.FoodItems.FindAsync(id);
    if (food == null)
    {
        return NotFound();
    }

    food.IsActive = !food.IsActive;
    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = food.IsActive ? "Món ăn đã được mở lại." : "Món ăn đã bị khóa.";
    return RedirectToAction("Index");
}

    [HttpGet]
    public async Task<IActionResult> ActivityLogs()
    {
        var logs = await (from activity in _context.UserActivities
                          join user in _context.Users
                          on activity.UserId equals user.Id
                          orderby activity.LastLoginTime descending
                          select new
                          {
                              UserId = activity.UserId,
                              UserName = user.UserName,
                              LastLoginTime = activity.LastLoginTime
                          }).ToListAsync();

        // Chuyển sang ViewModel nếu cần
        var viewModel = logs.Select(x => new ActivityLogViewModel
        {
            UserId = x.UserId,
            UserName = x.UserName,
            LastLoginTime = x.LastLoginTime
        }).ToList();

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Alerts()
    {
        var alerts = await _context.SystemAlerts
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
        return View(alerts);
    }

    [HttpPost]
    public async Task<IActionResult> AddAlert(SystemAlert alert)
    {
        if (ModelState.IsValid)
        {
            alert.CreatedAt = DateTime.Now;
            _context.SystemAlerts.Add(alert);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Alerts));
    }

    [HttpPost]
    public async Task<IActionResult> UpdateAlert(SystemAlert alert)
    {
        var existing = await _context.SystemAlerts.FindAsync(alert.Id);
        if (existing != null)
        {
            existing.Message = alert.Message;
            existing.Severity = alert.Severity;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Alerts));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteAlert(int id)
    {
        var alert = await _context.SystemAlerts.FindAsync(id);
        if (alert != null)
        {
            _context.SystemAlerts.Remove(alert);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Alerts));
    }

   
    [HttpGet]
    public async Task<IActionResult> Content()
    {
        var items = await _context.ContentItems.ToListAsync();
        return View(items);
    }

    [HttpPost]
    public async Task<IActionResult> AddContent(ContentItem item)
    {
        item.CreatedAt = DateTime.Now;
        _context.ContentItems.Add(item);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Content));
    }

  
    [HttpGet]
    public async Task<IActionResult> Media()
    {
        var media = await _context.MediaFiles.ToListAsync();
        return View(media);
    }

    [HttpPost]
    public async Task<IActionResult> AddMedia(MediaFile media)
    {
        if (ModelState.IsValid)
        {
            media.UploadedAt = DateTime.Now;
            _context.MediaFiles.Add(media);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Media));
    }

    [HttpPost]
    public async Task<IActionResult> UpdateMedia(MediaFile media)
    {
        var existing = await _context.MediaFiles.FindAsync(media.Id);
        if (existing != null)
        {
            existing.FileName = media.FileName;
            existing.FileType = media.FileType;
            existing.Url = media.Url;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Media));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteMedia(int id)
    {
        var media = await _context.MediaFiles.FindAsync(id);
        if (media != null)
        {
            _context.MediaFiles.Remove(media);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Media));
    }
    [HttpGet]
    public async Task<IActionResult> Settings()
    {
        var settings = await _context.SiteSettings.ToListAsync(); // ✅ Trả về List<SiteSetting>
        return View(settings);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSetting(string Key, string Value)
    {
        if (string.IsNullOrWhiteSpace(Key))
        {
            TempData["Error"] = "Khoá không được để trống.";
            return RedirectToAction(nameof(Settings));
        }

        var exists = await _context.SiteSettings.AnyAsync(s => s.Key == Key);
        if (exists)
        {
            TempData["Error"] = $"Khoá '{Key}' đã tồn tại.";
            return RedirectToAction(nameof(Settings));
        }

        _context.SiteSettings.Add(new SiteSetting { Key = Key, Value = Value });
        await _context.SaveChangesAsync();

        TempData["Message"] = $"Đã thêm thiết lập '{Key}' thành công.";
        return RedirectToAction(nameof(Settings));
    }

    [HttpPost]
    public async Task<IActionResult> UpdateSetting(int id, string key, string value)
    {
        var setting = await _context.SiteSettings.FindAsync(id);
        if (setting == null)
        {
            TempData["Error"] = "Không tìm thấy thiết lập.";
            return RedirectToAction(nameof(Settings));
        }

        if (setting.Key != key)
        {
            var exists = await _context.SiteSettings.AnyAsync(s => s.Key == key && s.Id != id);
            if (exists)
            {
                TempData["Error"] = $"Khoá '{key}' đã tồn tại.";
                return RedirectToAction(nameof(Settings));
            }
            setting.Key = key;
        }

        setting.Value = value;
        await _context.SaveChangesAsync();

        TempData["Message"] = $"Đã cập nhật thiết lập '{key}' thành công.";
        return RedirectToAction(nameof(Settings));
    }

    [HttpPost]
    public async Task<IActionResult> UploadLogo(IFormFile logoFile)
    {
        if (logoFile != null && logoFile.Length > 0)
        {
            var fileName = Path.GetFileName(logoFile.FileName);
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/logos");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);

            var setting = await _context.SiteSettings.FirstOrDefaultAsync(s => s.Key == "LogoUrl");
            if (setting != null && !string.IsNullOrEmpty(setting.Value))
            {
                var oldLogoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", setting.Value.TrimStart('/'));
                if (System.IO.File.Exists(oldLogoPath))
                    System.IO.File.Delete(oldLogoPath);
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await logoFile.CopyToAsync(stream);
            }

            var logoUrl = "/uploads/logos/" + fileName;

            if (setting != null)
                setting.Value = logoUrl;
            else
                _context.SiteSettings.Add(new SiteSetting { Key = "LogoUrl", Value = logoUrl });

            await _context.SaveChangesAsync();
            TempData["Message"] = "Logo mới đã được tải lên và logo cũ đã bị xóa!";
        }
        else
        {
            TempData["Error"] = "Không có file được chọn.";
        }

        return RedirectToAction(nameof(Settings));
    }

    [HttpPost]
    public async Task<IActionResult> UpdateBanner(string BannerTitle, string BannerSubtitle)
    {
        await UpsertSetting("BannerTitle", BannerTitle);
        await UpsertSetting("BannerSubtitle", BannerSubtitle);

        TempData["Message"] = "Nội dung banner đã được cập nhật!";
        return RedirectToAction(nameof(Settings));
    }

    [HttpPost]
    public async Task<IActionResult> UpdateContact(string Hotline, string FacebookUrl, string InstagramUrl)
    {
        await UpsertSetting("Hotline", Hotline);
        await UpsertSetting("FacebookUrl", FacebookUrl);
        await UpsertSetting("InstagramUrl", InstagramUrl);

        TempData["Message"] = "Thông tin liên hệ đã được cập nhật!";
        return RedirectToAction(nameof(Settings));
    }

    [HttpPost]
    public async Task<IActionResult> UpdateCompanyInfo(string CompanyName, string FoundedYear, string Mission, string Address, string Email, string WorkingHours)
    {
        await UpsertSetting("CompanyName", CompanyName);
        await UpsertSetting("FoundedYear", FoundedYear);
        await UpsertSetting("Mission", Mission);
        await UpsertSetting("Address", Address);
        await UpsertSetting("Email", Email);
        await UpsertSetting("WorkingHours", WorkingHours);

        TempData["Message"] = "Thông tin công ty đã được cập nhật!";
        return RedirectToAction(nameof(Settings));
    }

    private async Task UpsertSetting(string key, string value)
    {
        var setting = await _context.SiteSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting != null)
            setting.Value = value;
        else
            _context.SiteSettings.Add(new SiteSetting { Key = key, Value = value });

        await _context.SaveChangesAsync();
    }
    [HttpPost]
    public async Task<IActionResult> UpdateFooter(string FooterText, string FooterCopyright, string FooterNote)
    {
        await UpsertSetting("FooterText", FooterText);
        await UpsertSetting("FooterCopyright", FooterCopyright);
        await UpsertSetting("FooterNote", FooterNote);

        TempData["Message"] = "Nội dung footer đã được cập nhật!";
        return RedirectToAction(nameof(Settings));
    }


    [HttpGet]
    public async Task<IActionResult> Backup()
    {
        var backups = await _context.BackupRecords
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
        return View(backups);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBackup(string fileName)
    {
        if (!string.IsNullOrEmpty(fileName))
        {
            var backup = new BackupRecord
            {
                FileName = fileName,
                CreatedAt = DateTime.Now,
                IsRestored = false
            };
            _context.BackupRecords.Add(backup);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Backup));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteBackup(int id)
    {
        var backup = await _context.BackupRecords.FindAsync(id);
        if (backup != null)
        {
            _context.BackupRecords.Remove(backup);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Backup));
    }
    public IActionResult Admin()
    {
        return View();
    }
    public IActionResult Create()
    {
        ViewBag.CategoryList = new SelectList(_context.Categories, "Id", "Name");
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Create(FoodItem model)
    {
        ViewBag.CategoryList = new SelectList(_context.Categories, "Id", "Name");


        if (model.ImageFile != null)
        {
            string fileName = Path.GetFileNameWithoutExtension(model.ImageFile.FileName);
            string extension = Path.GetExtension(model.ImageFile.FileName);
            fileName = fileName + "_" + Guid.NewGuid().ToString() + extension;

            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await model.ImageFile.CopyToAsync(stream);
            }

            model.ImageUrl = "/images/" + fileName;
        }

        _context.FoodItems.Add(model);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }

   


    [HttpGet]
    public IActionResult Edit(int id)
    {
        var item = _context.FoodItems.Find(id);
        if (item == null) return NotFound();

        ViewBag.CategoryList = new SelectList(_context.Categories, "Id", "Name", item.CategoryId);
        return View(item);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(FoodItem model)
    {
        ViewBag.CategoryList = new SelectList(_context.Categories, "Id", "Name", model.CategoryId);

        var existing = await _context.FoodItems.FindAsync(model.Id);
        if (existing == null) return NotFound();


        existing.Name = model.Name;
        existing.Description = model.Description;
        existing.Price = model.Price;
        existing.CategoryId = model.CategoryId;

        

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Đã lưu thay đổi món ăn thành công!";
        return RedirectToAction("Edit", new { id = model.Id });
    }


    [HttpGet]
    public async Task<IActionResult> DeleteFood(int? id)
    {
        if (id == null) return NotFound();

        var item = await _context.FoodItems.FindAsync(id);
        if (item == null) return NotFound();

        return View(item);
    }

    // POST: Xác nhận xóa món ăn
    [HttpPost, ActionName("DeleteFood")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        // Tìm món ăn kèm các OrderDetails liên quan
        var item = await _context.FoodItems.FirstOrDefaultAsync(f => f.Id == id);
        if (item != null)
        {
            // Xóa các OrderDetails liên quan
            var relatedOrders = await _context.OrderDetails
                .Where(o => o.FoodItemId == item.Id)
                .ToListAsync();

            if (relatedOrders.Any())
            {
                _context.OrderDetails.RemoveRange(relatedOrders);
            }

            // Xóa món ăn
            _context.FoodItems.Remove(item);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "✅ Đã xóa món ăn!";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> UpdateOrderStatus(int id, string actionType)
    {
        var userId = _userManager.GetUserId(User);
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (order == null)
        {
            TempData["OrderMessage"] = "❌ Không tìm thấy đơn hàng.";
            return RedirectToAction("MyOrder");
        }

        switch (actionType)
        {
            case "cancel":
                if (order.Status == "Đang xử lý")
                {
                    order.Status = "Đã hủy";
                    await _context.SaveChangesAsync();
                    TempData["OrderMessage"] = $"✅ Đơn hàng #{order.Id} đã được hủy thành công.";
                }
                else
                {
                    TempData["OrderMessage"] = $"⚠️ Đơn hàng #{order.Id} không thể hủy vì đã được xử lý.";
                }
                break;

            case "confirm":
                if (order.Status == "Đã giao hàng")
                {
                    order.Status = "Đã nhận hàng";
                    await _context.SaveChangesAsync();
                    TempData["OrderMessage"] = $"✅ Bạn đã xác nhận nhận hàng cho đơn #{order.Id}.";
                }
                else if (order.Status == "Đã nhận hàng")
                {
                    TempData["OrderMessage"] = $"ℹ️ Đơn hàng #{order.Id} đã được xác nhận trước đó.";
                }
                else
                {
                    TempData["OrderMessage"] = $"⚠️ Đơn hàng #{order.Id} chưa được giao, không thể xác nhận.";
                }
                break;

            default:
                TempData["OrderMessage"] = "❌ Hành động không hợp lệ.";
                break;
        }

        return RedirectToAction("MyOrder");
    }


    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DiscountCodes()
    {
        var codes = await _context.DiscountCode.OrderByDescending(d => d.Id).ToListAsync();
        return View(codes); // Truyền danh sách mã giảm giá vào View
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DiscountCodes(string Code, decimal Amount, DateTime? ExpiryDate, bool NoExpiry)
    {
        if (!NoExpiry && (!ExpiryDate.HasValue || ExpiryDate.Value < DateTime.Today))
        {
            TempData["Error"] = "❌ Ngày hết hạn không hợp lệ.";
            return RedirectToAction("DiscountCodes");
        }

        var discount = new DiscountCode
        {
            Code = Code,
            Amount = Amount,
            ExpiryDate = NoExpiry ? null : ExpiryDate,
            IsActive = true
        };

        _context.DiscountCode.Add(discount);
        await _context.SaveChangesAsync();

        TempData["Success"] = "✅ Mã giảm giá đã được thêm!";
        return RedirectToAction("DiscountCodes");
    }
   public async Task<IActionResult> Monan()
{
    var items = await _context.FoodItems
        .Include(f => f.Category)
        .Select(f => new FoodItem
        {
            Id = f.Id,
            Name = f.Name,
            Price = f.Price,
            Description = f.Description,
            ImageUrl = f.ImageUrl,
            Category = f.Category,
            TotalQuantity = f.TotalQuantity,
            QuantitySold = _context.OrderDetails
                .Where(o => o.FoodItemId == f.Id)
                .Sum(o => (int?)o.Quantity) ?? 0
        })
        .ToListAsync();

    return View(items);
}

[HttpPost]
public async Task<IActionResult> UpdateQuantity(int Id, int TotalQuantity)
{
    var food = await _context.FoodItems.FindAsync(Id);
    if (food != null)
    {
        food.TotalQuantity = TotalQuantity;
        _context.Update(food);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Cập nhật số lượng thành công!";
    }
    return RedirectToAction("Monan");
}

}

