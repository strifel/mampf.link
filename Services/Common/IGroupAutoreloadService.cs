namespace GroupOrder.Services.Common;

using Data;

public interface IGroupAutoreloadService {

    public GroupEventHandlerHolder getHandlerForGroup(Group group);
}

public class GroupEventHandlerHolder {
    public event EventHandler OnGroupUpdated;

    public void Call()
    {
        OnGroupUpdated.Invoke(this, EventArgs.Empty);
    }
}