using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace GroupOrder.Data;

using System.ComponentModel;

[Index(nameof(Slug), IsUnique = true)]
public class VanityURL
{
    public int Id { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "Name can not exceed 100 characters.")]
    public string? VanityName { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "Slug cannot exceed 100 characters."), RegularExpression("[a-z0-9-]+", ErrorMessage = "Slug may only contain a-z, numbers and dashes.")]
    public string? Slug { get; set; }
    
    [Required]
    [StringLength(15, ErrorMessage = "Code cannot exceed 15 characters."), RegularExpression("[a-z0-9-]+", ErrorMessage = "Slug may only contain a-z and numbers.")]
    public string? AdminCode { get; set; }
    
    public DateTime UsedAt { get; set; }
    
    public ICollection<Group> History { get; } = new List<Group>();
}