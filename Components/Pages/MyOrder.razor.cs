using GroupOrder.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace GroupOrder.Components.Pages;

public partial class MyOrder
{
    private int? _personId;
    private string? NewName { get; set; }
    private string? OrderFood { get; set; }
    private decimal? OrderPrice { get; set; }
    private EditContext? _editContext;
    private ValidationMessageStore? _messageStore;
    private bool _showAllOrders;
    private bool NoPerson { get; set; }
    private PaymentMethod? _selectedPaymentMethod;

    [Parameter]
    public string? GroupSlug { get; set; }

    protected override void OnInitialized()
    {
        _editContext = new(this);
        _editContext.OnValidationRequested += HandleValidationRequested;
        _messageStore = new(_editContext);
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
        if (GroupService.CurrentGroup != null && _personId == null)
        {
            var personId = await ProtectedLocalStorage.GetAsync<int>(
                "grouporder_person_" + GroupService.CurrentGroup.Id
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
        if (_selectedPaymentMethod == null)
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
        payment.PaymentMethod = _selectedPaymentMethod ?? PaymentMethod.Other;

        GroupService.AddPayment(payment, person);

        await GroupService.Save();
        GroupService.ReloadRestriction.Release();
        _selectedPaymentMethod = null;
    }

    private int GetPriceToPay()
    {
        if (_personId == null)
            return 0;
        if (GroupService.CurrentGroup == null)
            return 0;
        if (GroupService.CurrentGroup.PaymentType != PaymentType.Pay)
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
        if (GroupService.CurrentGroup == null)
            return;
        if (NewName == null)
            return;
        if (NewName.Length is > 100 or 0)
            return;

        GroupService.ReloadRestriction.WaitOne();

        Person person = new Person { Group = GroupService.CurrentGroup, Name = NewName };

        GroupService.AddPerson(person);
        GroupService.Save();

        _personId = person.Id;

        GroupService.ReloadRestriction.Release();

        ProtectedLocalStorage.SetAsync("grouporder_person_" + GroupService.CurrentGroup.Id, person.Id);
    }

    private void HandleValidationRequested(object? sender, ValidationRequestedEventArgs args)
    {
        _messageStore?.Clear();

        if (string.IsNullOrEmpty(OrderFood) || OrderFood.Length > 100)
        {
            _messageStore?.Add(
                () => _editContext!,
                "You must enter a Food Choice between 1 and 100 chars."
            );
        }

        if (GroupService.CurrentGroup is null || GroupService.CurrentGroup.PaymentType != PaymentType.NoPrices)
        {
            if (OrderPrice == null | OrderPrice < 0)
            {
                _messageStore?.Add(() => _editContext!, "Price must be greater than 0.");
            }

            if (OrderPrice is > 2000)
            {
                // We produce an overflow if its bigger than 21474836€
                // We also produce overflows if the sum of the total order is bigger than 21474836€
                _messageStore?.Add(() => _editContext!, "Price must be smaller or equal to 2000€.");
            }
        }

        if (IsOrderingClosed())
        {
            _messageStore?.Add(() => _editContext!, "The ordering period has already closed.");
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

        if (GroupService.CurrentGroup!.PaymentType != PaymentType.NoPrices)
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
        _editContext!.OnValidationRequested -= HandleValidationRequested;
        _editContext = new(this);
        _editContext.OnValidationRequested += HandleValidationRequested;
        _messageStore = new(_editContext);
    }

    public void Dispose()
    {
        if (_editContext is not null)
        {
            _editContext.OnValidationRequested -= HandleValidationRequested;
        }

        GroupService.OnGroupReload -= GroupServiceOnGroupReload;
    }

    private bool IsOrderingClosed()
    {
        return DateTime.Now > GroupService.CurrentGroup!.ClosingTime;
    }
}
