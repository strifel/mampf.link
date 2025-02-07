namespace GroupOrder.Services.Common;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.WebUtilities;

public class GroupAdminService(NavigationManager navManager, IGroupService groupService)
    : IAdminService
{
    private string? _adminCode;
    private bool _initialized = false;

    public event EventHandler? CodeChanged;

    public void Initialize()
    {
        if (_initialized)
            return;
        navManager.LocationChanged += HandleLocationChanged;
        LoadAdminCode();
        _initialized = true;
    }

    public bool IsAdmin()
    {
        if (_adminCode == null)
            return false;
        if (groupService.CurrentGroup == null)
            return false;

        return groupService.CurrentGroup.AdminCode == _adminCode;
    }

    private void HandleLocationChanged(object? sender, LocationChangedEventArgs? e)
    {
        var oldCode = _adminCode;
        LoadAdminCode();
        if (oldCode != _adminCode)
        {
            CodeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void LoadAdminCode()
    {
        if (
            navManager.Uri.Contains("?")
            && QueryHelpers
                .ParseQuery(navManager.Uri.Split("?")[1])
                .TryGetValue("admin", out var code)
        )
        {
            _adminCode = code;
        }
        else
        {
            _adminCode = null;
        }
    }
}
