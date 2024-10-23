namespace GroupOrder.Services.Common;

using Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.WebUtilities;

public class GroupAdminService(NavigationManager nm, IGroupService groupService) : IAdminService {

    private String? _adminCode;
    private bool _initialized = false;

    public void Initialize()
    {
        if (_initialized) return;
        nm.LocationChanged += HandleLocationChanged;
        LoadAdminCode();
        _initialized = true;
    }
    
    public bool IsAdmin()
    {
        if (_adminCode == null) return false;
        if (groupService.Group == null) return false;
        
        return groupService.Group.AdminCode == _adminCode;
    }
    
    
    private void HandleLocationChanged(object? sender, LocationChangedEventArgs? e)
    {
        LoadAdminCode();
    }

    private void LoadAdminCode()
    {
        if (nm.Uri.Contains("?") && QueryHelpers.ParseQuery(nm.Uri.Split("?")[1]).TryGetValue("admin", out var code))
        {
            _adminCode = code;
        }
        else
        {
            _adminCode = null;
        }
    }
}
