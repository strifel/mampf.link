using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace GroupOrder.Data;

using System.ComponentModel;

[Index(nameof(GroupSlug), IsUnique = true)]
public class Group
{
    public int Id { get; set; }

    [Required(ErrorMessage = "A group name is required.")]
    [StringLength(100, ErrorMessage = "Group name length cannot exceed 100 characters.")]
    public string? GroupName { get; set; }

    [Required]
    [
        StringLength(100, ErrorMessage = "Group slug length cannot exceed 100 characters."),
        RegularExpression(
            "[a-z0-9-]+",
            ErrorMessage = "Slug may only contain lower case letters, numbers and dashes."
        )
    ]
    public string? GroupSlug { get; set; }

    [Required]
    [
        StringLength(16, ErrorMessage = "Admin code length cannot exceed 16 characters."),
        RegularExpression("[a-z0-9-]+", ErrorMessage = "Slug may only contain a-z and numbers.")
    ]
    public string? AdminCode { get; set; }

    [StringLength(500, ErrorMessage = "Description length cannot exceed 500 characters.")]
    public string? Description { get; set; }

    [
        StringLength(30, ErrorMessage = "PayPal username length cannot exceed 30 characters."),
        RegularExpression(
            "[a-z0-9-]+",
            ErrorMessage = "PayPal username may only contain a-z and numbers."
        )
    ]
    public string? PaypalUsername { get; set; }

    [
        StringLength(500, ErrorMessage = "Menu URL length cannot exceed 500 characters."),
        Url(ErrorMessage = "Menu URL is not valid.")
    ]
    public string? MenuUrl { get; set; }

    [Required]
    public DateTime? ClosingTime { get; set; }

    [Required, DefaultValue(false)]
    public bool PayAfterClosingTime { get; set; } = false;

    [
        Required(ErrorMessage = "A shipping cost is required, but can be 0."),
        DefaultValue(0),
        Range(0, 10000, ErrorMessage = "Shipping cost has to be between 0 € and 100 €.")
    ]
    public int ShippingCost { get; set; } = 0;

    [RegularExpression(
        "^([A-Z]{2}[ \\-]?[0-9]{2})(?=(?:[ \\-]?[A-Z0-9]){9,30}$)((?:[ \\-]?[A-Z0-9]{3,5}){2,7})([ \\-]?[A-Z0-9]{1,3})?$",
        ErrorMessage = "IBAN is not valid."
    )]
    public string? IBAN { get; set; }

    [StringLength(100, ErrorMessage = "Bank name length cannot exceed 100 characters.")]
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

        return string.Join(
            "BREAK",
            [
                "BCD", // has to be BCD
                "002", // 002 = EWR, so no BIC needed
                "1", // 1=UTF-8
                "INST", // Instant payment
                "", // no BIC
                BankName, // legal name, will be checked by bank
                IBAN.Replace(" ", "").Replace("-", ""), // IBAN
                $"EUR{Order.GetPrice(person.GetPriceToPay())}", // Amount
                "", // Purpose (DTA)
                "", // Remittance Reference (ISO 11649 RF)
                $"{GroupName} ({person.Name})", // Remittance Text
                "", // Information
            ]
        );
    }

    public int GetShippingCostPerPerson()
    {
        int payingPersons = Persons.Count(p => p.Orders.Count > 0);
        if (payingPersons == 0)
            return ShippingCost;
        return ShippingCost / payingPersons;
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
