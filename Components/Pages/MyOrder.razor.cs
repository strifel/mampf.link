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
        gs.OnGroupReload += GsOnOnGroupReload;
        base.OnInitialized();
    }

    private void GsOnOnGroupReload(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // find local person
        // this needs to be done here because it uses javascript interop
        if (gs.Group != null && _personId == null)
        {
            var personId = await ProtectedLocalStorage.GetAsync<int>(
                "grouporder_person_" + gs.Group.Id
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
        gs.ReloadRestriction.WaitOne();
        Person? person = gs.GetPersonByID(_personId.Value);
        if (person == null)
        {
            gs.ReloadRestriction.Release();
            return;
        }

        Payment payment = new Payment();
        payment.Amount = paid;
        payment.Person = person;
        payment.PaymentConfirmed = false;
        payment.PaymentMethod = SelectedPaymentMethod ?? PaymentMethod.Other;

        gs.AddPayment(payment, person);

        await gs.Save();
        gs.ReloadRestriction.Release();
        SelectedPaymentMethod = null;
    }

    private int GetPriceToPay()
    {
        if (_personId == null)
            return 0;
        if (gs.Group == null)
            return 0;
        if (gs.Group.PaymentType != PaymentType.PAY)
            return 0;
        Person? person = gs.GetPersonByID(_personId.Value);
        if (person == null)
        {
            return 0;
        }

        return person.GetPriceToPay();
    }

    private void CreatePerson()
    {
        if (gs.Group == null)
            return;
        if (NewName == null)
            return;
        if (NewName.Length is > 100 or 0)
            return;

        gs.ReloadRestriction.WaitOne();

        Person person = new Person { Group = gs.Group, Name = NewName };

        gs.AddPerson(person);
        gs.Save();

        _personId = person.Id;

        gs.ReloadRestriction.Release();

        ProtectedLocalStorage.SetAsync("grouporder_person_" + gs.Group.Id, person.Id);
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

        if (gs.Group is null || gs.Group.PaymentType != PaymentType.NO_PRICES)
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
        gs.ReloadRestriction.WaitOne();
        Person? person = gs.GetPersonByID(_personId.Value);
        if (person == null)
        {
            gs.ReloadRestriction.Release();
            return;
        }

        Order newOrder = new() { Food = OrderFood! };

        if (gs.Group!.PaymentType != PaymentType.NO_PRICES)
        {
            newOrder.Price = (int)(OrderPrice! * 100);
        }

        if (adminService.IsAdmin())
        {
            Payment payment = new Payment();
            payment.PaymentMethod = PaymentMethod.NoPayment;
            payment.Amount = newOrder.Price;
            payment.Person = person;
            payment.PaymentNote = "Auto added due to group leader order";
            payment.PaymentConfirmed = false;
            gs.AddPayment(payment, person);
        }

        gs.AddOrder(newOrder, person);
        await gs.Save();
        await gs.ReloadGroup();

        gs.ReloadRestriction.Release();

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

        gs.OnGroupReload -= GsOnOnGroupReload;
    }

    private bool IsOrderingClosed()
    {
        return DateTime.Now > gs.Group!.ClosingTime;
    }
}
