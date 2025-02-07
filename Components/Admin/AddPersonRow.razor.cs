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
        if (GroupService.Group == null)
            return;
        if (PersonName == null)
            return;
        if (PersonName.Length is > 100 or 0)
            return;

        GroupService.ReloadRestriction.WaitOne();

        Person person = new Person { Group = GroupService.Group, Name = PersonName };

        GroupService.AddPerson(person);
        await GroupService.Save();

        GroupService.ReloadRestriction.Release();
        PersonName = null;
        await OnAdded.InvokeAsync();
    }
}
