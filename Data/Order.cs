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
}