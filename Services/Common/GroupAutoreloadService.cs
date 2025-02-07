namespace GroupOrder.Services.Common;

using Data;

public class GroupAutoreloadService : IGroupAutoreloadService
{
    private readonly Dictionary<int, GroupEventHandlerHolder> _handlers = new();

    public GroupEventHandlerHolder GetHandlerForGroup(Group group)
    {
        if (_handlers.TryGetValue(group.Id, out var holder))
        {
            return holder;
        }

        _handlers[group.Id] = new();
        return _handlers[group.Id];
    }
}
