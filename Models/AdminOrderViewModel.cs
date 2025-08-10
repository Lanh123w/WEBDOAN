namespace WEBDOAN.Models;
public class AdminOrderViewModel
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = "Không có tên";
    public string Address { get; set; } = "Chưa có địa chỉ";
    public string Phone { get; set; } = "Chưa có số";
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = "Chưa xác định";
    public decimal TotalAmount { get; set; }
    public decimal ItemTotal { get; set; }
    public string? DiscountCode { get; set; } 
    public string? Note { get; set; }        
    public List<OrderDetail> OrderDetails { get; set; } = new();
}
