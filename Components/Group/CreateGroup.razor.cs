using System.Security.Cryptography;
using GroupOrder.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace GroupOrder.Components.Group;

public partial class CreateGroup
{
    public string? OrgName { get; set; } = null;

    private EditContext? _editContext;

    [SupplyParameterFromForm] private Data.Group? Model { get; set; }

    private ValidationMessageStore? _messageStore;

    protected override void OnInitialized()
    {
        Model ??= new()
        {
            ClosingTime = DateTime.Now + TimeSpan.FromMinutes(30),
            EditingRule = EditingRule.NeverAllow,
        };

        _editContext = new(Model);
        _editContext.OnValidationRequested += HandleValidationRequested;
        _messageStore = new(_editContext);
    }

    private void HandleValidationRequested(object? sender, ValidationRequestedEventArgs e)
    {
        _messageStore?.Clear();

        if (Model == null)
        {
            return;
        }

        Model.GroupSlug ??= RandomNumberGenerator.GetHexString(10, true);
        Model.AdminCode ??= RandomNumberGenerator.GetHexString(16, true);

        // Check if not both are filled/empty
        if (
            (Model.BankName == null && Model.IBAN != null)
            || (Model.BankName != null && Model.IBAN == null)
        )
        {
            _messageStore?.Add(
                () => Model,
                "Bank name and IBAN must both be empty or both be filled."
            );
        }

        if (Model.ClosingTime < DateTime.Now)
        {
            _messageStore?.Add(() => Model.ClosingTime,
                "Closing time must be in the future.");
        }

        if (
            Model.EditingRule == EditingRule.AllowBeforeCartAndPaymentAndDeadline
            && Model.PaymentType != PaymentType.Pay
        )
        {
            _messageStore?.Add(() => Model.EditingRule,
                "Editing Rule is invalid, please reselect.");
        }
    }

    public void Dispose()
    {
        if (_editContext is not null)
        {
            _editContext.OnValidationRequested -= HandleValidationRequested;
        }
    }

    private void Submit()
    {
        using var context = DbContextFactory.CreateDbContext();
        context.Add(Model!);
        context.SaveChanges();

        NavigationManager.NavigateTo(
            $"/group/{Model!.GroupSlug}/overview?admin={Model!.AdminCode}"
        );
    }
}