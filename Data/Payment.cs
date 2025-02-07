using System.ComponentModel.DataAnnotations;

namespace GroupOrder.Data;

public class Payment
{
    public int Id { get; set; }

    [Required]
    public Person Person { get; set; } = null!;

    [Required]
    public int? Amount { get; set; }

    [Required]
    public bool? PaymentConfirmed { get; set; }

    [Required]
    public PaymentMethod PaymentMethod { get; set; }

    [StringLength(100, ErrorMessage = "Payment Note can not exceed 100 characters.")]
    public string? PaymentNote { get; set; }
}

public enum PaymentMethod
{
    Cash,
    WireTransfer,
    Paypal,
    Refund, // For when the group leaders returns overpaid money
    Other,
    NoPayment, // For when the group leader orders themself
}
