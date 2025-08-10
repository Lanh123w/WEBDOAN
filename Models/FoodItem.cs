using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBDOAN.Models
{
    public class FoodItem
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        [Range(0, 500000)]
        public decimal Price { get; set; }
       
        public string ImageUrl { get; set; }

        [NotMapped] 
        public IFormFile ImageFile { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; }

        [NotMapped] 
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
