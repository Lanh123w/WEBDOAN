using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Data;

namespace WEBDOAN.Components
{
    public class SiteSettingViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public SiteSettingViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var settings = await _context.SiteSettings.ToListAsync();

            return View(settings); // ✅ Truyền List<SiteSetting>
        }
    }
}
