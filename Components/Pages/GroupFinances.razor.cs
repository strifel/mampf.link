using GroupOrder.Data;
using Microsoft.AspNetCore.Components;

namespace GroupOrder.Components.Pages;

public partial class GroupFinances
{
    [Parameter]
    public string? GroupSlug { get; set; }

    private bool HideConfirmedPaid = false;

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

    private IEnumerable<Person> GetPersons()
    {
        if (GroupService.CurrentGroup == null)
        {
            return [];
        }

        IEnumerable<Person> persons = GroupService.CurrentGroup!.Persons;
        persons = persons
            .Where(p => p.Orders.Sum(x => x.Price) != 0 || p.Payments.Count != 0)
            .OrderBy(x => x.Name);
        if (HideConfirmedPaid)
        {
            persons = persons.Where(p => p.GetPriceToPay(true) != 0);
        }

        return persons;
    }

    public void Dispose()
    {
        GroupService.OnGroupReload -= GroupServiceOnGroupReload;
        AdminService.CodeChanged -= HandleAdminCodeChanged;
    }

    private void Paid(Person person)
    {
        GroupService.ReloadRestriction.WaitOne();
        Payment payment = new Payment();
        payment.PaymentConfirmed = true;
        payment.Person = person;
        payment.PaymentMethod = PaymentMethod.Other;
        payment.PaymentNote = "Payment added by group leader";
        payment.Amount = person.GetPriceToPay();
        GroupService.AddPayment(payment, person);
        GroupService.Save();
        GroupService.ReloadRestriction.Release();
    }

    private void PaidBack(Person person)
    {
        GroupService.ReloadRestriction.WaitOne();
        Payment payment = new Payment();
        payment.PaymentConfirmed = true;
        payment.Person = person;
        payment.PaymentMethod = PaymentMethod.Refund;
        payment.PaymentNote = "Payment added by group leader";
        payment.Amount = person.GetPriceToPay();
        GroupService.AddPayment(payment, person);
        GroupService.Save();
        GroupService.ReloadRestriction.Release();
    }

    private void HandleAdminCodeChanged(object? sender, EventArgs e)
    {
        StateHasChanged();
    }
}
