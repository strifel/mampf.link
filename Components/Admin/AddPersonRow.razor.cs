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
        if (GroupService.CurrentGroup == null)
            return;
        if (PersonName == null)
            return;
        if (PersonName.Length is > 100 or 0)
            return;

        await GroupService.CreateNewPerson(PersonName);

        PersonName = null;
        await OnAdded.InvokeAsync();
    }
}
