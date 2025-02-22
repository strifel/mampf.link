using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using RouteData = Microsoft.AspNetCore.Components.RouteData;

namespace GroupOrder.Components.Layout;

public partial class AppLayout
{
    [CascadingParameter]
    private RouteData? RouteData { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (RouteData?.RouteValues.ContainsKey("GroupSlug") == true)
        {
            await GroupService.LoadGroup(
                (string)RouteData?.RouteValues.GetValueOrDefault("GroupSlug")!
            );
        }
        await AdminService.Initialize();
        await base.OnParametersSetAsync();
        NavManager.LocationChanged += HandleLocationChanged;
        AdminService.CodeChanged += HandleCodeChanged;
    }

    private async void HandleLocationChanged(object? sender, LocationChangedEventArgs? e)
    {
        if (RouteData?.RouteValues.ContainsKey("GroupSlug") != true)
            return; // TODO clear
        await GroupService.LoadGroup(
            (string)RouteData?.RouteValues.GetValueOrDefault("GroupSlug")!
        );
        StateHasChanged();
    }

    private async void HandleCodeChanged(object? sender, EventArgs e)
    {
        await GroupService.ReloadGroup();
        StateHasChanged();
    }
}
