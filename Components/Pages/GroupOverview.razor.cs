using GroupOrder.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace GroupOrder.Components.Pages;

public partial class GroupOverview
{
    [Parameter]
    public string? GroupSlug { get; set; }

    private string? newTitle;
    private string? newDescription;
    private DateTime? newDeadline;
    private string? newMenuURL;
    private bool fail = false;
    private bool addPerson = false;
    private String? OrderFood { get; set; }
    private Decimal? OrderPrice { get; set; }
    private int? OrderPersonId { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        NavManager.LocationChanged += HandleLocationChanged;
        GroupService.OnGroupReload += HandleGroupChanged;
    }

    private void HandleLocationChanged(object? sender, LocationChangedEventArgs? e)
    {
        StateHasChanged();
    }

    private void HandleGroupChanged(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    private async void ChangeTitle(ChangeEventArgs x)
    {
        if (x.Value is string && GroupService.CurrentGroup != null)
        {
            newTitle = (string)x.Value;
        }
    }

    private async void SaveTitle()
    {
        if (newTitle == null || newTitle.Length <= 0 || newTitle.Length > 100)
        {
            GroupService.ReloadRestriction.Release();
            fail = true;
            return;
        }

        if (GroupService.CurrentGroup != null)
        {
            GroupService.CurrentGroup.GroupName = newTitle;
            await GroupService.Save();
            fail = false;
        }

        GroupService.ReloadRestriction.Release();
    }

    private async void ChangeDescription(ChangeEventArgs x)
    {
        if (x.Value is string && GroupService.CurrentGroup != null)
        {
            newDescription = (string)x.Value;
        }
    }

    private async void SaveDescription()
    {
        if (newDescription == null || newDescription.Length > 500)
        {
            GroupService.ReloadRestriction.Release();
            fail = true;
            return;
        }

        if (GroupService.CurrentGroup != null)
        {
            GroupService.CurrentGroup.Description = newDescription;
            await GroupService.Save();
            fail = false;
        }

        GroupService.ReloadRestriction.Release();
    }

    private async void ChangeMenuURL(ChangeEventArgs x)
    {
        if (x.Value is string && GroupService.CurrentGroup != null)
        {
            newMenuURL = (string)x.Value;
        }
    }

    private async void SaveMenuURL()
    {
        if (newMenuURL != null && !Uri.IsWellFormedUriString(newMenuURL, UriKind.Absolute))
        {
            GroupService.ReloadRestriction.Release();
            fail = true;
            return;
        }

        if (GroupService.CurrentGroup != null)
        {
            GroupService.CurrentGroup.MenuUrl = newMenuURL;
            await GroupService.Save();
            fail = false;
        }

        GroupService.ReloadRestriction.Release();
    }

    private async void ChangeDeadline(ChangeEventArgs x)
    {
        if (x.Value is String && GroupService.CurrentGroup != null)
        {
            newDeadline = DateTime.Parse((string)x.Value!);
        }
    }

    private async void SaveDeadline()
    {
        if (GroupService.CurrentGroup != null)
        {
            GroupService.CurrentGroup.ClosingTime = newDeadline;
            await GroupService.Save();
        }

        GroupService.ReloadRestriction.Release();
    }

    private void Delete(Order order)
    {
        GroupService.ReloadRestriction.WaitOne();
        GroupService.DeleteOrder(order);
        GroupService.Save();
        GroupService.ReloadRestriction.Release();
    }

    private void AddToOrder()
    {
        if (GroupService.CurrentGroup == null)
        {
            fail = true;
            return;
        }

        if (
            GroupService.CurrentGroup!.PaymentType != PaymentType.NoPrices
            && OrderPrice is null or < 0
        )
        {
            fail = true;
            return;
        }

        if (OrderPrice is > 2000)
        {
            fail = true;
            return;
        }

        if (string.IsNullOrEmpty(OrderFood) || OrderFood.Length > 50)
        {
            fail = true;
            return;
        }

        Order order = new() { Food = OrderFood };

        if (OrderPersonId == null)
        {
            if (GroupService.CurrentGroup.Persons.Count > 0)
            {
                order.Person = GroupService.CurrentGroup.Persons.First();
            }
            else
            {
                fail = true;
                return;
            }
        }
        else
        {
            order.Person = GroupService.CurrentGroup.Persons.Single(
                (person) => person.Id == OrderPersonId
            );
        }

        if (OrderPrice != null)
        {
            order.Price = (int)(OrderPrice! * 100);
        }

        GroupService.ReloadRestriction.WaitOne();
        GroupService.AddOrder(order, order.Person);
        GroupService.Save();
        GroupService.ReloadRestriction.Release();

        fail = false;
    }

    public void Dispose()
    {
        NavManager.LocationChanged -= HandleLocationChanged;
        GroupService.OnGroupReload -= HandleGroupChanged;
    }
}
