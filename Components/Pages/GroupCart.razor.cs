using GroupOrder.Data;
using Microsoft.AspNetCore.Components;

namespace GroupOrder.Components.Pages;

public partial class GroupCart
{
    [Parameter]
    public string? GroupSlug { get; set; }

    private Stack<int> _undoStack = new();

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
        _undoStack.Push(order.Id);
        GroupService.ReloadRestriction.Release();
    }

    private async void UndoAdd()
    {
        if (_undoStack.Count == 0)
            return;
        if (GroupService.CurrentGroup == null)
            return;
        if (!AdminService.IsAdmin())
            return; // this theoretically should not happen
        GroupService.ReloadRestriction.WaitOne();

        int orderId = _undoStack.Pop();
        Order? order = GroupService.CurrentGroup.Orders.FirstOrDefault((order) => order.Id == orderId);
        
        if (order == null)
        {
            GroupService.ReloadRestriction.Release();
            return;
        }

        order.AddedToCart = false;
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
        
        _undoStack.Clear();
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
