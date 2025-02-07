using System.Security.Cryptography;
using System.Text.RegularExpressions;
using GroupOrder.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace GroupOrder.Components.Pages;

public partial class CreateGroup
{
    [SupplyParameterFromForm]
    public Data.Group? Model { get; set; }
    private ValidationMessageStore? _messageStore;
    private EditContext? _editContext;

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

    private void Submit()
    {
        using var context = DbFactory.CreateDbContext();
        Model!.AdminCode = RandomNumberGenerator.GetHexString(10);
        Model!.GroupSlug = RandomNumberGenerator.GetHexString(10);
        context.Add(Model);
        context.SaveChanges();

        NavigationManager.NavigateTo(
            $"/group/{Model!.GroupSlug}/overview?admin={Model!.AdminCode}"
        );
    }

    private void HandleValidationRequested(object? sender, ValidationRequestedEventArgs args)
    {
        _messageStore?.Clear();

        if (
            Model!.GroupName == null
            || Model!.GroupName!.Length == 0
            || Model!.GroupName!.Length > 100
        )
        {
            _messageStore?.Add(() => Model!, "You must enter a Name between 1 and 100 chars.");
        }

        if (Model!.Description != null && Model!.Description!.Length > 500)
        {
            _messageStore?.Add(() => Model!, "Description should be maximum 500 characters.");
        }

        if (
            Model!.PaypalUsername != null
            && (
                Model!.PaypalUsername!.Length > 20
                || !new Regex("^[a-zA-Z0-9]+$").IsMatch(Model!.PaypalUsername!)
            )
        )
        {
            _messageStore?.Add(() => Model!, "Please enter a valid paypal username.");
        }

        if (Model!.ClosingTime == null || Model!.ClosingTime < DateTime.Now)
        {
            _messageStore?.Add(() => Model!, "Please select an Closing Time in the future.");
        }

        if (Model!.MenuUrl != null && !Uri.IsWellFormedUriString(Model!.MenuUrl, UriKind.Absolute))
        {
            _messageStore?.Add(() => Model!, "Please enter no or a valid url.");
        }

        if (
            Model!.EditingRule == EditingRule.AllowBeforeCartAndDeadline
            && Model!.PaymentType == PaymentType.Pay
        )
        {
            _messageStore?.Add(() => Model!, "Please select a valid Editing Rule.");
        }

        if (
            Model!.EditingRule == EditingRule.AllowBeforeCartAndPaymentAndDeadline
            && Model!.PaymentType != PaymentType.Pay
        )
        {
            _messageStore?.Add(() => Model!, "Please select a valid Editing Rule.");
        }
    }

    public void Dispose()
    {
        if (_editContext is not null)
        {
            _editContext.OnValidationRequested -= HandleValidationRequested;
        }
    }
}
