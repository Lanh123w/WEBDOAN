using System.ComponentModel.DataAnnotations;

namespace WEBDOAN.Models;
public class DiscountCode
{
    public int Id { get; set; }

    [Required]
    public string Code { get; set; }

    public decimal Amount { get; set; } // số tiền giảm

    public DateTime? ExpiryDate { get; set; }

    public bool IsActive { get; set; }
}
