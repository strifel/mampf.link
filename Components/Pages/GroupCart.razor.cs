using GroupOrder.Data;
using Microsoft.AspNetCore.Components;

namespace GroupOrder.Components.Pages;

public partial class GroupCart
{
    [Parameter]
    public string? GroupSlug { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        GroupService.OnGroupReload += GroupServiceOnGroupReload;
    }

    private void GroupServiceOnGroupReload(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    private int CalculateTotalSum()
    {
        if (GroupService.Group == null)
            return 0;
        return GroupService.Group.Orders.Sum(o => o.Price) ?? 0;
    }

    private int CalculateAddedSum()
    {
        if (GroupService.Group == null)
            return 0;
        return GroupService.Group.Orders.Sum(o => o.AddedToCart ?? false ? o.Price : 0) ?? 0;
    }

    private async void SetAdded(Order order)
    {
        if (GroupService.Group == null)
            return;
        if (!AdminService.IsAdmin())
            return; // this theoretically should not happen
        GroupService.ReloadRestriction.WaitOne();
        order.AddedToCart = true;
        await GroupService.Save();
        GroupService.ReloadRestriction.Release();
    }

    private async void ResetCart()
    {
        if (GroupService.Group == null)
            return;
        if (!AdminService.IsAdmin())
            return; // this theoretically should not happen
        GroupService.ReloadRestriction.WaitOne();
        foreach (Order order in GroupService.Group.Orders)
        {
            order.AddedToCart = false;
        }

        await GroupService.Save();
        GroupService.ReloadRestriction.Release();
    }
}
