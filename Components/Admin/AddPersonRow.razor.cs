using GroupOrder.Data;
using Microsoft.AspNetCore.Components;

namespace GroupOrder.Components.Admin;

public partial class AddPersonRow
{
    private String? PersonName { get; set; }

    [Parameter]
    public EventCallback OnAdded { get; set; }

    private async void AddPerson()
    {
        if (gs.Group == null)
            return;
        if (PersonName == null)
            return;
        if (PersonName.Length is > 100 or 0)
            return;

        gs.ReloadRestriction.WaitOne();

        Person person = new Person { Group = gs.Group, Name = PersonName };

        gs.AddPerson(person);
        await gs.Save();

        gs.ReloadRestriction.Release();
        PersonName = null;
        await OnAdded.InvokeAsync();
    }
}
