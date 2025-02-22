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
    [
        StringLength(100, ErrorMessage = "Slug cannot exceed 100 characters."),
        RegularExpression(
            "[a-z0-9-]+",
            ErrorMessage = "Slug may only contain a-z, numbers and dashes."
        )
    ]
    public string? GroupSlug { get; set; }

    [Required]
    [
        StringLength(15, ErrorMessage = "Code cannot exceed 15 characters."),
        RegularExpression("[a-z0-9-]+", ErrorMessage = "Slug may only contain a-z and numbers.")
    ]
    public string? AdminCode { get; set; }

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? Description { get; set; }

    [
        StringLength(30, ErrorMessage = "PaypalUsername cannot exceed 30 characters."),
        RegularExpression(
            "[a-z0-9-]+",
            ErrorMessage = "Paypal Username may only contain a-z and numbers."
        )
    ]
    public string? PaypalUsername { get; set; }

    [StringLength(100, ErrorMessage = "MenuURL cannot exceed 100 characters.")]
    public string? MenuUrl { get; set; }

    [Required]
    public DateTime? ClosingTime { get; set; }

    public string? IBAN { get; set; }

    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string? BankName { get; set; }

    public ICollection<Order> Orders { get; } = new List<Order>();
    public ICollection<Person> Persons { get; } = new List<Person>();

    [Required]
    [DefaultValue(PaymentType.Pay)]
    public PaymentType PaymentType { get; set; }

    [Required]
    [DefaultValue(EditingRule.NeverAllow)]
    public EditingRule EditingRule { get; set; }

    // ReSharper disable once InconsistentNaming
    public string? GetPaymentQR(Person person)
    {
        if (BankName == null || IBAN == null)
            return null;

        return "BCDBREAK"
            + // has to be BCD
            "002BREAK"
            + // 002 = EWR, so no BIC needed
            "1BREAK"
            + // 1=UTF-8
            "INSTBREAK"
            + // Instant payment
            "BREAK"
            + // no BIC
            BankName
            + "BREAK"
            + // legal name, will be checked by bank
            IBAN.Replace(" ", "").Replace("-", "")
            + "BREAK"
            + // IBAN
            "EUR"
            + Order.GetPrice(person.GetPriceToPay())
            + "BREAK"
            + // Amount
            "BREAK"
            + // Purpose (DTA)
            "BREAK"
            + // Remittance Reference (ISO 11649 RF)
            GroupName
            + " ("
            + person.Name
            + ")BREAK"; // Remittance Text
        // Information
    }
}

public enum PaymentType
{
    Pay,
    NoNeedToPay,
    NoPrices,
}

public enum EditingRule
{
    AllowBeforeDeadline,
    AllowBeforeCartAndDeadline,
    AllowBeforeCartAndPaymentAndDeadline,
    AskEverytime, //TODO implement pending deletes
    NeverAllow,
}
