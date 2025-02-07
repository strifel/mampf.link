using GroupOrder.Data;
using Microsoft.AspNetCore.Components;

namespace GroupOrder.Components.Pages;

public partial class GroupCart
{
    [Parameter] public string? GroupSlug { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        gs.OnGroupReload += GsOnOnGroupReload;
    }

    private void GsOnOnGroupReload(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    private int CalculateTotalSum()
    {
        if (gs.Group == null) return 0;
        return gs.Group.Orders.Sum(o => o.Price) ?? 0;
    }

    private int CalculateAddedSum()
    {
        if (gs.Group == null) return 0;
        return gs.Group.Orders.Sum(o => o.AddedToCart ?? false ? o.Price : 0) ?? 0;
    }

    private async void SetAdded(Order order)
    {
        if (gs.Group == null) return;
        if (!adminService.IsAdmin()) return; // this theoretically should not happen
        gs.ReloadRestriction.WaitOne();
        order.AddedToCart = true;
        await gs.Save();
        gs.ReloadRestriction.Release();
    }

    private async void ResetCart()
    {
        if (gs.Group == null) return;
        if (!adminService.IsAdmin()) return; // this theoretically should not happen
        gs.ReloadRestriction.WaitOne();
        foreach (Order order in gs.Group.Orders)
        {
            order.AddedToCart = false;
        }

        await gs.Save();
        gs.ReloadRestriction.Release();
    }
}