using GroupOrder.Data;

namespace GroupOrder.Components.MyOrder;

public partial class CreatePerson
{
    private string? NewName { get; set; }

    private void OnJoinClick()
    {
        if (GroupService.CurrentGroup == null)
            return;
        if (NewName == null)
            return;
        if (NewName.Length is > 100 or 0)
            return;

        GroupService.CreateNewPerson(NewName);

        ProtectedLocalStorage.SetAsync(
            "grouporder_person_" + GroupService.CurrentGroup.Id,
            GroupService.CurrentPerson!.Id
        );
    }
}
