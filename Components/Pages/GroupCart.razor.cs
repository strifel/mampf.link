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
        AdminService.CodeChanged += HandleAdminCodeChanged;
    }

    private void GroupServiceOnGroupReload(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    private int CalculateTotalSum()
    {
        if (GroupService.CurrentGroup == null)
            return 0;
        return GroupService.CurrentGroup.Orders.Sum(o => o.Price) ?? 0;
    }

    private int CalculateAddedSum()
    {
        if (GroupService.CurrentGroup == null)
            return 0;
        return GroupService.CurrentGroup.Orders.Sum(o => o.AddedToCart ?? false ? o.Price : 0) ?? 0;
    }

    private async void SetAdded(Order order)
    {
        if (GroupService.CurrentGroup == null)
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
        if (GroupService.CurrentGroup == null)
            return;
        if (!AdminService.IsAdmin())
            return; // this theoretically should not happen
        GroupService.ReloadRestriction.WaitOne();
        foreach (Order order in GroupService.CurrentGroup.Orders)
        {
            order.AddedToCart = false;
        }

        await GroupService.Save();
        GroupService.ReloadRestriction.Release();
    }

    public void Dispose()
    {
        GroupService.OnGroupReload -= GroupServiceOnGroupReload;
        AdminService.CodeChanged -= HandleAdminCodeChanged;
    }

    private void HandleAdminCodeChanged(object? sender, EventArgs e)
    {
        StateHasChanged();
    }
}
