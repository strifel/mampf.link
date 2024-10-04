using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace GroupOrder.Data;

using System.ComponentModel;

[Index(nameof(GroupSlug), IsUnique = true)]
public class Group
{
    public int Id { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "Name can not exceed 100 characters.")]
    public string? GroupName { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "Slug cannot exceed 100 characters."), RegularExpression("[a-z0-9-]+", ErrorMessage = "Slug may only contain a-z, numbers and dashes.")]
    public string? GroupSlug { get; set; }
    
    [Required]
    [StringLength(15, ErrorMessage = "Code cannot exceed 15 characters."), RegularExpression("[a-z0-9-]+", ErrorMessage = "Slug may only contain a-z and numbers.")]
    public string? AdminCode { get; set; }
    
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? Description { get; set; }
    
    [StringLength(30, ErrorMessage = "PaypalUsername cannot exceed 30 characters."), RegularExpression("[a-z0-9-]+", ErrorMessage = "Paypal Username may only contain a-z and numbers.")]
    public string? PaypalUsername { get; set; }
    
    public ICollection<Order> Orders { get; } = new List<Order>();
    public ICollection<Person> Persons { get; } = new List<Person>();
    
    [Required]
    [DefaultValue(PaymentType.PAY)]
    public PaymentType PaymentType { get; set; }
    
}

public enum PaymentType {
    PAY,
    NO_NEED_TO_PAY,
    NO_PRICES
}