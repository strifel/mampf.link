using System.ComponentModel.DataAnnotations;

namespace GroupOrder.Data;

public class Person
{
    // The id is used to identify which person a given user can edit
    // therefore we will sign a cookie with the id
    public int Id { get; set; }
    
    // One person can only ever exist in exactly one Group
    [Required]
    public Group Group { get; set; } = null!;
    
    [Required]
    [StringLength(100, ErrorMessage = "Name can not exceed 100 characters.")]
    public string? Name { get; set; }
    
    // One Person can only ever have Orders from the correct group
    public ICollection<Order> Orders { get; } = new List<Order>();

}