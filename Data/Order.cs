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
    public Person Person { get; set; } = null!;

    [Required]
    [StringLength(100, ErrorMessage = "Food cannot exceed 100 characters.")]
    public string? Food { get; set; }

    [Required]
    [DefaultValue(false)]
    public bool? AddedToCart { get; set; }

    [Required]
    public int? Price { get; set; } // in cent

    public String GetPrice()
    {
        return GetPrice(this.Price ?? 0);
    }

    public static String GetPrice(int price)
    {
        return (price / 100) + "." + $"{price % 100:D2}";
    }
}
