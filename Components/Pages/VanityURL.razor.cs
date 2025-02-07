using System.Security.Cryptography;
using System.Text.RegularExpressions;
using GroupOrder.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;

namespace GroupOrder.Components.Pages;

public partial class VanityURL
{
    [SupplyParameterFromForm] public Data.Group? Model { get; set; }
    [SupplyParameterFromForm] public String? VanityAdminCodeInput { get; set; }
    private ValidationMessageStore? _messageStore;

    [Parameter] public string? VanitySlug { get; set; }

    private GroupContext? _context;
    private EditContext? _editContext;
    private bool Loading { get; set; } = false;
    private bool NotFound { get; set; } = false;
    public Data.VanityURL? Vanity;

    protected override async Task OnParametersSetAsync()
    {
        await this.LoadVanityAsync();
    }

    protected override void OnInitialized()
    {
        Model ??= new()
        {
            ClosingTime = DateTime.Now + TimeSpan.FromMinutes(30),
            EditingRule = EditingRule.NeverAllow
        };

        _editContext = new(Model);
        _editContext.OnValidationRequested += HandleValidationRequested;
        _messageStore = new(_editContext);
    }

    private async Task LoadVanityAsync()
    {
        if (Loading)
        {
            return; //avoid concurrent requests.
        }

        Vanity = null;
        Loading = true;

        if (_context == null)
        {
            _context = DbFactory.CreateDbContext();
        }

        Vanity = await _context.VanityUrls
            .Include(v => v.History)
            .SingleOrDefaultAsync(
                c => c.Slug == VanitySlug);

        if (Vanity is null)
        {
            NotFound = true;
        }
        else
        {
            if (Model is { GroupName: null })
            {
                Model.GroupName = Vanity.VanityName + " " + DateTime.Now.Day + "." +
                                  DateTime.Now.Month;
            }
        }

        Loading = false;
    }

    private void Submit()
    {
        if (Vanity == null) return;
        if (_context == null) return;

        // Remove old Vanity

        if (Vanity.History.Count > 0)
        {
            Vanity.History.Last().GroupSlug = RandomNumberGenerator.GetHexString(10);
        }

        // Add new Vanity

        Model!.AdminCode = RandomNumberGenerator.GetHexString(10);
        Model!.GroupSlug = Vanity.Slug;
        Vanity.History.Add(Model);
        Vanity.UsedAt = DateTime.Now;
        _context.Add(Model);
        _context.SaveChanges();

        NavigationManager.NavigateTo(
            $"/group/{Model!.GroupSlug}/overview?admin={Model!.AdminCode}");
    }

    private void HandleValidationRequested(object? sender,
        ValidationRequestedEventArgs args)
    {
        _messageStore?.Clear();

        if (Model!.GroupName == null || Model!.GroupName!.Length == 0 ||
            Model!.GroupName!.Length > 100)
        {
            _messageStore?.Add(() => Model!, "You must enter a Name between 1 and 100 chars.");
        }

        if (Model!.Description != null && Model!.Description!.Length > 500)
        {
            _messageStore?.Add(() => Model!, "Description should be maximum 500 characters");
        }

        if (Model!.PaypalUsername != null && (Model!.PaypalUsername!.Length > 20 ||
                                              !new Regex("^[a-zA-Z0-9]+$").IsMatch(
                                                  Model!.PaypalUsername!)))
        {
            _messageStore?.Add(() => Model!, "Please enter a valid paypal username");
        }

        if (VanityAdminCodeInput == null || Vanity == null ||
            !VanityAdminCodeInput.Equals(Vanity!.AdminCode))
        {
            _messageStore?.Add(() => Model!, "Please enter the correct Vanity-URL password");
        }

        if (Model!.ClosingTime == null || Model!.ClosingTime < DateTime.Now)
        {
            _messageStore?.Add(() => Model!, "Please select an Closing Time in the future.");
        }

        if (Model!.MenuUrl != null && !Uri.IsWellFormedUriString(Model!.MenuUrl, UriKind.Absolute))
        {
            _messageStore?.Add(() => Model!, "Please enter no or a valid url.");
        }

        if (Model!.EditingRule == EditingRule.AllowBeforeCartAndDeadline &&
            Model!.PaymentType == PaymentType.Pay)
        {
            _messageStore?.Add(() => Model!, "Please select a valid Editing Rule.");
        }

        if (Model!.EditingRule == EditingRule.AllowBeforeCartAndPaymentAndDeadline &&
            Model!.PaymentType != PaymentType.Pay)
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

    private int getExistingTimeOfCurrentGroup()
    {
        return (int)(DateTime.Now - Vanity!.UsedAt).TotalMinutes;
    }
}