namespace GroupOrder.Services.Common;

using Data;

public class GroupAutoreloadService : IGroupAutoreloadService
{
    private readonly Dictionary<int, GroupEventHandlerHolder> _handlers = new();

    public GroupEventHandlerHolder getHandlerForGroup(Group group)
    {
        if (_handlers.TryGetValue(group.Id, out var holder))
        {
            return holder;
        }
        else
        {
            _handlers[group.Id] = new();
            return _handlers[group.Id];
        }
    }
}
