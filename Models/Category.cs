using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        // Navigation property (optional)
        public ICollection<FoodItem> FoodItems { get; set; }
    }
}
