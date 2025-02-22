using System.Security.Cryptography;
using GroupOrder.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace GroupOrder.Components.Group;

public partial class CreateGroup
{
    [Parameter]
    public string? OrgName { get; set; }

    [Parameter]
    public string? OrgPassword { get; set; }

    [Parameter]
    public Action<Data.Group>? CustomSubmitCallback { get; set; }

    [Parameter]
    public string? DefaultGroupName { get; set; }

    private EditContext? _editContext;

    [SupplyParameterFromForm]
    private Data.Group? Model { get; set; }

    [SupplyParameterFromForm]
    public string? OrgPasswordInput { get; set; }

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

    protected override void OnParametersSet()
    {
        if (Model != null && DefaultGroupName != null)
        {
            Model.GroupName = DefaultGroupName;
        }
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
            _messageStore?.Add(() => Model.ClosingTime, "Closing time must be in the future.");
        }

        if (
            Model.EditingRule == EditingRule.AllowBeforeCartAndPaymentAndDeadline
            && Model.PaymentType != PaymentType.Pay
        )
        {
            _messageStore?.Add(
                () => Model.EditingRule,
                "Editing Rule is invalid, please reselect."
            );
        }

        if (OrgPassword != null && OrgPassword != OrgPasswordInput)
        {
            _messageStore?.Add(() => OrgPasswordInput!, "Organization password is invalid.");
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
        if (CustomSubmitCallback is not null)
        {
            Logger.LogDebug("Calling CustomSubmitCallback");
            CustomSubmitCallback(Model!);
        }
        else
        {
            using var context = DbContextFactory.CreateDbContext();
            context.Add(Model!);
            context.SaveChanges();

            NavigationManager.NavigateTo(
                $"/group/{Model!.GroupSlug}/overview?admin={Model!.AdminCode}"
            );
        }
    }
}
