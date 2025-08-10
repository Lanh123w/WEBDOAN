
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Data;
using WebApplication3.Models;
using WEBDOAN.Models;


[Authorize]
public class CartController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public CartController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // ✅ Hiển thị giỏ hàng
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var cartItems = await _context.CartItems
            .Include(c => c.FoodItem)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        return View(cartItems);
    }

    // ✅ Thêm món vào giỏ hàng và hiển thị thông báo
    [HttpPost]
    public async Task<IActionResult> AddToCart(int foodItemId, int quantity = 1)
    {
        var userId = _userManager.GetUserId(User);

        var existing = await _context.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.FoodItemId == foodItemId);

        if (existing != null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            _context.CartItems.Add(new CartItem
            {
                UserId = userId,
                FoodItemId = foodItemId,
                Quantity = quantity
            });
        }

        await _context.SaveChangesAsync();

        var item = await _context.FoodItems.FindAsync(foodItemId);
        TempData["Message"] = $"{item?.Name} đã được thêm vào giỏ hàng!";
        return RedirectToAction("ItemsByCategory", "FoodItems", new { id = item?.CategoryId });
    }
    [HttpGet]
    public async Task<IActionResult> NhapDiaChi()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var cart = _context.CartItems
            .Include(c => c.FoodItem)
            .Where(c => c.UserId == user.Id)
            .ToList();

        return View(cart);
    }

    // ✅ Đặt hàng
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> DatHang(string CustomerName, string Address, string Phone, string? DiscountCode)
    {
        // Kiểm tra thông tin giao hàng
        if (string.IsNullOrWhiteSpace(Address) || string.IsNullOrWhiteSpace(CustomerName) || string.IsNullOrWhiteSpace(Phone))
        {
            TempData["Error"] = "Vui lòng nhập đầy đủ thông tin giao hàng.";
            return RedirectToAction("NhapDiaChi");
        }

        var userId = _userManager.GetUserId(User);

        // Lấy giỏ hàng
        var cartItems = await _context.CartItems
            .Include(c => c.FoodItem)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (!cartItems.Any())
        {
            TempData["Message"] = "Giỏ hàng của bạn đang trống.";
            return RedirectToAction("Cart");
        }

        // Tính tổng tiền ban đầu
        decimal originalAmount = cartItems.Sum(c => c.Quantity * c.FoodItem.Price);
        decimal discountAmount = 0;

        // Kiểm tra mã giảm giá
        if (!string.IsNullOrWhiteSpace(DiscountCode))
        {
            var discount = await _context.DiscountCode
                .FirstOrDefaultAsync(d => d.Code == DiscountCode && d.IsActive && d.ExpiryDate > DateTime.Now);

            if (discount != null)
            {
                discountAmount = discount.Amount;
            }
            else
            {
                TempData["Error"] = "Mã giảm giá không hợp lệ hoặc đã hết hạn.";
                return RedirectToAction("NhapDiaChi");
            }
        }

        // Tính tổng tiền sau giảm
        decimal totalAmount = originalAmount - discountAmount;
        if (totalAmount < 0) totalAmount = 0;

        string defaultStatus = "Chờ xác nhận";


        // Tạo đơn hàng
        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.Now,
            CustomerName = CustomerName,
            Address = Address,
            Phone = Phone,
            Status = defaultStatus,
            DiscountCode = DiscountCode,
            DiscountAmount = discountAmount,
            OriginalAmount = originalAmount,
            TotalAmount = totalAmount,
            OrderDetails = cartItems.Select(c => new OrderDetail
            {
                FoodItemId = c.FoodItemId,
                Quantity = c.Quantity,
                UnitPrice = c.FoodItem.Price
            }).ToList()
        };

        // Lưu đơn hàng và xóa giỏ hàng
        _context.Orders.Add(order);
        _context.CartItems.RemoveRange(cartItems);
        await _context.SaveChangesAsync();

        // Chuyển đến trang thành công
        return RedirectToAction("OrderSuccess", new { orderId = order.Id });
    }


    public async Task<IActionResult> ThanhToan()
    {
        var userId = _userManager.GetUserId(User); // Lấy ID người dùng hiện tại

        var cartItems = await _context.CartItems
            .Include(c => c.FoodItem)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (!cartItems.Any())
        {
            TempData["Error"] = "Giỏ hàng trống!";
            return RedirectToAction("Cart");
        }

        return View(cartItems); // View: ThanhToan.cshtml
    }
    [HttpPost]
    public async Task<IActionResult> UpdateCart(List<CartItem> CartItems)
    {
        var userId = _userManager.GetUserId(User);

        foreach (var item in CartItems)
        {
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.FoodItemId == item.FoodItemId);

            if (cartItem != null)
            {
                cartItem.Quantity = item.Quantity;
            }
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật giỏ hàng!";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> OrderSuccess(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.FoodItem)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }
    public async Task<IActionResult> MyOrder()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var orders = await _context.Orders
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.FoodItem)
            .Where(o => o.UserId == user.Id)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return View(orders);
    }
    [Authorize]
    public async Task<IActionResult> OrderDetail(int id)
    {
        var order = await _context.Orders
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.FoodItem)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        // Kiểm tra người dùng có quyền xem đơn hàng này
        var user = await _userManager.GetUserAsync(User);
        if (order.UserId != user.Id)
        {
            return Forbid();
        }

        return View(order);
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

    // ✅ Xóa sản phẩm khỏi giỏ hàng
    [HttpPost]
    public async Task<IActionResult> RemoveFormCart(int id)
    {
        var userId = _userManager.GetUserId(User);

        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (cartItem != null)
        {
            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }

    

}
