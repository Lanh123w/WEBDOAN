using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Linq;
using WebApplication3.Data;
using WEBDOAN.Models;

namespace WebApplication3.Controllers
{
    public class FoodItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly Cloudinary _cloudinary;
        public FoodItemsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IOptions<CloudinarySettings> config)
        {
            _context = context;
            _userManager = userManager;
            var acc = new Account(config.Value.CloudName, config.Value.ApiKey, config.Value.ApiSecret);
            _cloudinary = new Cloudinary(acc);
        }

     

        public IActionResult Create()
        {
            ViewBag.CategoryList = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(FoodItem item)
        //{
        //    // Kiểm tra xem người dùng đã chọn ảnh chưa
        //    if (item.ImageFile == null || item.ImageFile.Length == 0)
        //    {
        //        ModelState.AddModelError("ImageUrl", "Ảnh món ăn là bắt buộc.");
        //    }
        //    else
        //    {
        //        // Upload ảnh lên Cloudinary (hoặc nơi lưu trữ khác)
        //        item.ImageUrl = await UploadImageToCloudinary(item.ImageFile);
        //    }

        //    // Kiểm tra xem CategoryId đã được chọn chưa
        //    if (item.CategoryId == 0)
        //    {
        //        ModelState.AddModelError("CategoryId", "Danh mục là bắt buộc.");
        //    }

        //    // Nếu ModelState không hợp lệ, trả về view với dữ liệu hiện tại
        //    if (!ModelState.IsValid)
        //    {
        //        ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", item.CategoryId);
        //        return View(item);
        //    }

        //    // Lưu món ăn vào database
        //    _context.Add(item);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}
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

        private async Task<string> UploadImageToCloudinary(IFormFile imageFile)
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(imageFile.FileName, imageFile.OpenReadStream()),
                Folder = "food_items"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.SecureUrl.ToString();
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

            // Nếu có ảnh mới, upload lên Cloudinary
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                string imageUrl = await UploadImageToCloudinary(model.ImageFile);
                existing.ImageUrl = imageUrl;
            }
           

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã lưu thay đổi món ăn thành công!";
            return RedirectToAction("Edit", new { id = model.Id });
        }


        // GET: Xóa món ăn
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.FoodItems.FindAsync(id);
            if (item == null) return NotFound();

            return View(item);
        }

        // POST: Xác nhận xóa
        [HttpPost, ActionName("Delete")]
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

        // GET: /FoodItems
        public async Task<IActionResult> Index()
        {
            var items = await _context.FoodItems.Include(f => f.Category).ToListAsync();
            return View(items);
        }

        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }

 public async Task<IActionResult> ItemsByCategory(int id)
{
    var category = await _context.Categories
        .FirstOrDefaultAsync(c => c.Id == id);

    if (category == null)
        return NotFound();

    var foodItems = await _context.FoodItems
        .Where(f => f.CategoryId == id)
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
                .Sum(o => (int?)o.Quantity) ?? 0,
            IsActive = f.IsActive
        })
        .ToListAsync();

    category.FoodItems = foodItems;

    return View(category);
}


     public async Task<IActionResult> AllItems()
{
    var allItems = await _context.FoodItems
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

    // Tính số lượng còn lại và lọc món còn hàng
    var filteredItems = allItems
        .Where(f => (f.TotalQuantity - f.QuantitySold) > 0)
        .ToList();

    return View(filteredItems);
}

    }
}
