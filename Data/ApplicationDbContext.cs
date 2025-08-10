using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WEBDOAN.Models;


namespace WebApplication3.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }
        public DbSet<FoodItem> FoodItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<DiscountCode> DiscountCode { get; set; }
        public DbSet<VisitLog> VisitLogs { get; set; }
        public DbSet<SystemAlert> SystemAlerts { get; set; }
        public DbSet<ContentItem> ContentItems { get; set; }
        public DbSet<MediaFile> MediaFiles { get; set; }
        public DbSet<SiteSetting> SiteSettings { get; set; }
        public DbSet<BackupRecord> BackupRecords { get; set; }
        public DbSet<UserActivity> UserActivities { get; set; }

     

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Quan hệ FoodItem → Category
            builder.Entity<FoodItem>()
                .HasOne(f => f.Category)
                .WithMany(c => c.FoodItems)
                .HasForeignKey(f => f.CategoryId);

            // Quan hệ CartItem → FoodItem
            builder.Entity<CartItem>()
                .HasOne(c => c.FoodItem)
                .WithMany(f => f.CartItems)
                .HasForeignKey(c => c.FoodItemId);
        }
    }
}
