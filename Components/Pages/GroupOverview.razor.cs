using GroupOrder.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace GroupOrder.Components.Pages;

public partial class GroupOverview
{
    [Parameter] public string? GroupSlug { get; set; }

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
        nm.LocationChanged += HandleLocationChanged;
        gs.OnGroupReload += HandleGroupChanged;
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
        if (x.Value is string && gs.Group != null)
        {
            newTitle = (string)x.Value;
        }
    }

    private async void SaveTitle()
    {
        if (newTitle == null || newTitle.Length <= 0 || newTitle.Length > 100)
        {
            gs.ReloadRestriction.Release();
            fail = true;
            return;
        }

        if (gs.Group != null)
        {
            gs.Group.GroupName = newTitle;
            await gs.Save();
            fail = false;
        }

        gs.ReloadRestriction.Release();
    }

    private async void ChangeDescription(ChangeEventArgs x)
    {
        if (x.Value is string && gs.Group != null)
        {
            newDescription = (string)x.Value;
        }
    }

    private async void SaveDescription()
    {
        if (newDescription == null || newDescription.Length > 500)
        {
            gs.ReloadRestriction.Release();
            fail = true;
            return;
        }

        if (gs.Group != null)
        {
            gs.Group.Description = newDescription;
            await gs.Save();
            fail = false;
        }

        gs.ReloadRestriction.Release();
    }

    private async void ChangeMenuURL(ChangeEventArgs x)
    {
        if (x.Value is string && gs.Group != null)
        {
            newMenuURL = (string)x.Value;
        }
    }

    private async void SaveMenuURL()
    {
        if (newMenuURL != null && !Uri.IsWellFormedUriString(newMenuURL, UriKind.Absolute))
        {
            gs.ReloadRestriction.Release();
            fail = true;
            return;
        }

        if (gs.Group != null)
        {
            gs.Group.MenuUrl = newMenuURL;
            await gs.Save();
            fail = false;
        }

        gs.ReloadRestriction.Release();
    }

    private async void ChangeDeadline(ChangeEventArgs x)
    {
        if (x.Value is String && gs.Group != null)
        {
            newDeadline = DateTime.Parse((string)x.Value!);
        }
    }

    private async void SaveDeadline()
    {
        if (gs.Group != null)
        {
            gs.Group.ClosingTime = newDeadline;
            await gs.Save();
        }

        gs.ReloadRestriction.Release();
    }

    private void Delete(Order order)
    {
        gs.ReloadRestriction.WaitOne();
        gs.DeleteOrder(order);
        gs.Save();
        gs.ReloadRestriction.Release();
    }

    private void AddToOrder()
    {
        if (gs.Group == null)
        {
            fail = true;
            return;
        }

        if (gs.Group!.PaymentType != PaymentType.NoPrices && OrderPrice is null or < 0)
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

        Order order = new()
        {
            Food = OrderFood
        };

        if (OrderPersonId == null)
        {
            if (gs.Group.Persons.Count > 0)
            {
                order.Person = gs.Group.Persons.First();
            }
            else
            {
                fail = true;
                return;
            }
        }
        else
        {
            order.Person = gs.Group.Persons.Single((person) => person.Id == OrderPersonId);
        }

        if (OrderPrice != null)
        {
            order.Price = (int)(OrderPrice! * 100);
        }

        gs.ReloadRestriction.WaitOne();
        gs.AddOrder(order, order.Person);
        gs.Save();
        gs.ReloadRestriction.Release();

        fail = false;
    }

    public void Dispose()
    {
        nm.LocationChanged -= HandleLocationChanged;
        gs.OnGroupReload -= HandleGroupChanged;
    }
}