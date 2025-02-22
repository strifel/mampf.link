namespace GroupOrder.Services.Common;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.WebUtilities;

public class GroupAdminService(
    NavigationManager navManager,
    IGroupService groupService,
    ProtectedLocalStorage protectedLocalStorage,
    ILogger<GroupService> logger
) : IAdminService
{
    private string? _adminCode;
    private bool _initialized = false;

    public event EventHandler? CodeChanged;

    public async Task Initialize()
    {
        if (_initialized)
            return;
        logger.LogDebug("Initializing admin service");
        navManager.LocationChanged += HandleLocationChanged;
        await LoadAdminCode();
        _initialized = true;
        CodeChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsAdmin()
    {
        if (_adminCode == null)
            return false;
        if (groupService.CurrentGroup == null)
            return false;

        return groupService.CurrentGroup.AdminCode == _adminCode;
    }

    private async void HandleLocationChanged(object? sender, LocationChangedEventArgs? e)
    {
        var oldCode = _adminCode;
        await LoadAdminCode();
        if (oldCode != _adminCode)
        {
            CodeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private async Task LoadAdminCode()
    {
        logger.LogDebug("Starting to fetch admin code");

        string? adminCode = null;

        // First try loading from local storage
        if (groupService.CurrentGroup != null)
        {
            adminCode = (
                await protectedLocalStorage.GetAsync<string>(
                    "grouporder_admin_" + groupService.CurrentGroup.Id
                )
            ).Value;
            logger.LogDebug(
                adminCode == null
                    ? "Did not find admin code in local storage"
                    : "Found admin code in local storage"
            );
        }
        else
        {
            logger.LogDebug("Not fetching from local storage as no group is known");
        }

        // Let URL override
        if (
            navManager.Uri.Contains("?")
            && QueryHelpers
                .ParseQuery(navManager.Uri.Split("?")[1])
                .TryGetValue("admin", out var code)
        )
        {
            logger.LogDebug("Found admin token in url");
            if (adminCode != code && groupService.CurrentGroup != null)
            {
                logger.LogDebug("Writing admin token to local storage");
                await protectedLocalStorage.SetAsync(
                    "grouporder_admin_" + groupService.CurrentGroup.Id,
                    (string)code!
                );
            }

            adminCode = code;
        }

        _adminCode = adminCode;
    }
}
