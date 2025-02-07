using GroupOrder.Data;
using Microsoft.AspNetCore.Components;

namespace GroupOrder.Components.Pages;

public partial class GroupFinanze
{
    [Parameter] public string? GroupSlug { get; set; }

    private bool HideConfirmedPaid = false;

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        gs.OnGroupReload += GsOnOnGroupReload;
    }

    private void GsOnOnGroupReload(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    private IEnumerable<Person> GetPersons()
    {
        if (gs.Group == null)
        {
            return [];
        }

        IEnumerable<Person> persons = gs.Group!.Persons;
        persons = persons.Where(p => p.Orders.Sum(x => x.Price) != 0 || p.Payments.Count != 0)
            .OrderBy(x => x.Name);
        if (HideConfirmedPaid)
        {
            persons = persons.Where(p => p.GetPriceToPay(true) != 0);
        }

        return persons;
    }

    public void Dispose()
    {
        gs.OnGroupReload -= GsOnOnGroupReload;
    }

    private void Paid(Person person)
    {
        gs.ReloadRestriction.WaitOne();
        Payment payment = new Payment();
        payment.PaymentConfirmed = true;
        payment.Person = person;
        payment.PaymentMethod = PaymentMethod.Other;
        payment.PaymentNote = "Payment added by group leader";
        payment.Amount = person.GetPriceToPay();
        gs.AddPayment(payment, person);
        gs.Save();
        gs.ReloadRestriction.Release();
    }

    private void PaidBack(Person person)
    {
        gs.ReloadRestriction.WaitOne();
        Payment payment = new Payment();
        payment.PaymentConfirmed = true;
        payment.Person = person;
        payment.PaymentMethod = PaymentMethod.Refund;
        payment.PaymentNote = "Payment added by group leader";
        payment.Amount = person.GetPriceToPay();
        gs.AddPayment(payment, person);
        gs.Save();
        gs.ReloadRestriction.Release();
    }
}