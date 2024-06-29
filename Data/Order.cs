using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace GroupOrder.Data;

public class Order
{
    public int Id { get; set; }
    
    [Required]
    public Group Group { get; set; } = null!;

    [Required]
    [StringLength(100, ErrorMessage = "Name can not exceed 100 characters.")]
    public string? Name { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "Food cannot exceed 100 characters.")]
    public string? Food { get; set; }
    
    [Required]
    public int? Price { get; set; } // in cent
    
    [Required]
    [DefaultValue(PaymentStatus.Unpaid)]
    public PaymentStatus PaymentStatus { get; set; }
    
    public String getPrice()
    {
        return (this.Price / 100) + "." + System.String.Format("{0:D2}", this.Price % 100);
    }
}

public enum PaymentStatus
{
    Unpaid,
    PaymentPending,
    Paid
}