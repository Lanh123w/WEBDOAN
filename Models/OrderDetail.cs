﻿namespace WEBDOAN.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int FoodItemId { get; set; }
        public FoodItem FoodItem { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

    }
}
