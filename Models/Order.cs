using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        [StringLength(100)]
        public string? CustomerName { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string UserId { get; set; } // để liên kết với người dùng
        public string Phone { get; set; }  // để lưu số điện thoại
        public decimal OriginalAmount { get; set; }
        public string? DiscountCode { get; set; } // có thể null

        public decimal DiscountAmount { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
