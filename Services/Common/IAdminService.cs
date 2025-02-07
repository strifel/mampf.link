namespace GroupOrder.Services.Common;

using Data;

public interface IAdminService
{
    // Is called on AdminCode changes
    public event EventHandler? CodeChanged;

    // Call this in OnParametersSet
    public void Initialize();

    // Returns if the user has admin privileges in
    // the current context (e.g. a group)
    public bool IsAdmin();
}
