using WebApplication3.Models;

public class CartItem
{
    public int Id { get; set; }

    public string UserId { get; set; } // từ Identity

    public int FoodItemId { get; set; }
    public FoodItem FoodItem { get; set; }

    public int Quantity { get; set; }
}
