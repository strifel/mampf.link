using GroupOrder.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace GroupOrder.Components.Pages;

public partial class MyOrder
{
    private int? _personId;
    private String? NewName { get; set; }
    private String? OrderFood { get; set; }
    private Decimal? OrderPrice { get; set; }
    private EditContext? editContext;
    private ValidationMessageStore? messageStore;
    private bool showAllOrders = false;
    private bool NoPerson { get; set; } = false;
    private PaymentMethod? SelectedPaymentMethod;

    [Parameter]
    public string? GroupSlug { get; set; }

    protected override void OnInitialized()
    {
        editContext = new(this);
        editContext.OnValidationRequested += HandleValidationRequested;
        messageStore = new(editContext);
        GroupService.OnGroupReload += GroupServiceOnGroupReload;
        base.OnInitialized();
    }

    private void GroupServiceOnGroupReload(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // find local person
        // this needs to be done here because it uses javascript interop
        if (GroupService.Group != null && _personId == null)
        {
            var personId = await ProtectedLocalStorage.GetAsync<int>(
                "grouporder_person_" + GroupService.Group.Id
            );
            if (personId.Success)
            {
                _personId = personId.Value;
            }
            else
            {
                NoPerson = true;
            }

            StateHasChanged();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private async void Paid()
    {
        if (_personId == null)
            return;
        if (SelectedPaymentMethod == null)
            return;
        int paid = GetPriceToPay();
        GroupService.ReloadRestriction.WaitOne();
        Person? person = GroupService.GetPersonByID(_personId.Value);
        if (person == null)
        {
            GroupService.ReloadRestriction.Release();
            return;
        }

        Payment payment = new Payment();
        payment.Amount = paid;
        payment.Person = person;
        payment.PaymentConfirmed = false;
        payment.PaymentMethod = SelectedPaymentMethod ?? PaymentMethod.Other;

        GroupService.AddPayment(payment, person);

        await GroupService.Save();
        GroupService.ReloadRestriction.Release();
        SelectedPaymentMethod = null;
    }

    private int GetPriceToPay()
    {
        if (_personId == null)
            return 0;
        if (GroupService.Group == null)
            return 0;
        if (GroupService.Group.PaymentType != PaymentType.Pay)
            return 0;
        Person? person = GroupService.GetPersonByID(_personId.Value);
        if (person == null)
        {
            return 0;
        }

        return person.GetPriceToPay();
    }

    private void CreatePerson()
    {
        if (GroupService.Group == null)
            return;
        if (NewName == null)
            return;
        if (NewName.Length is > 100 or 0)
            return;

        GroupService.ReloadRestriction.WaitOne();

        Person person = new Person { Group = GroupService.Group, Name = NewName };

        GroupService.AddPerson(person);
        GroupService.Save();

        _personId = person.Id;

        GroupService.ReloadRestriction.Release();

        ProtectedLocalStorage.SetAsync("grouporder_person_" + GroupService.Group.Id, person.Id);
    }

    private void HandleValidationRequested(object? sender, ValidationRequestedEventArgs args)
    {
        messageStore?.Clear();

        if (string.IsNullOrEmpty(OrderFood) || OrderFood.Length > 100)
        {
            messageStore?.Add(
                () => editContext!,
                "You must enter a Food Choice between 1 and 100 chars."
            );
        }

        if (GroupService.Group is null || GroupService.Group.PaymentType != PaymentType.NoPrices)
        {
            if (OrderPrice == null | OrderPrice < 0)
            {
                messageStore?.Add(() => editContext!, "Price must be greater than 0.");
            }

            if (OrderPrice is > 2000)
            {
                // We produce an overflow if its bigger than 21474836€
                // We also produce overflows if the sum of the total order is bigger than 21474836€
                messageStore?.Add(() => editContext!, "Price must be smaller or equal to 2000€.");
            }
        }

        if (IsOrderingClosed())
        {
            messageStore?.Add(() => editContext!, "The ordering period has already closed.");
        }
    }

    private async void AddToOrder()
    {
        if (_personId == null)
            return;
        GroupService.ReloadRestriction.WaitOne();
        Person? person = GroupService.GetPersonByID(_personId.Value);
        if (person == null)
        {
            GroupService.ReloadRestriction.Release();
            return;
        }

        Order newOrder = new() { Food = OrderFood! };

        if (GroupService.Group!.PaymentType != PaymentType.NoPrices)
        {
            newOrder.Price = (int)(OrderPrice! * 100);
        }

        if (AdminService.IsAdmin())
        {
            Payment payment = new Payment();
            payment.PaymentMethod = PaymentMethod.NoPayment;
            payment.Amount = newOrder.Price;
            payment.Person = person;
            payment.PaymentNote = "Auto added due to group leader order";
            payment.PaymentConfirmed = false;
            GroupService.AddPayment(payment, person);
        }

        GroupService.AddOrder(newOrder, person);
        await GroupService.Save();
        await GroupService.ReloadGroup();

        GroupService.ReloadRestriction.Release();

        ResetPendingOrder();
        StateHasChanged();
    }

    private void ResetPendingOrder()
    {
        OrderPrice = null;
        OrderFood = null;
        editContext!.OnValidationRequested -= HandleValidationRequested;
        editContext = new(this);
        editContext.OnValidationRequested += HandleValidationRequested;
        messageStore = new(editContext);
    }

    public void Dispose()
    {
        if (editContext is not null)
        {
            editContext.OnValidationRequested -= HandleValidationRequested;
        }

        GroupService.OnGroupReload -= GroupServiceOnGroupReload;
    }

    private bool IsOrderingClosed()
    {
        return DateTime.Now > GroupService.Group!.ClosingTime;
    }
}
