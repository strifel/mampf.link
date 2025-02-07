namespace GroupOrder.Services.Common;

using Data;
using Microsoft.EntityFrameworkCore;

public class GroupService(
    IDbContextFactory<GroupContext> dbFactory,
    IGroupAutoreloadService autoreloadService
) : IGroupService, IDisposable
{
    public event EventHandler? OnGroupReload;

    public Semaphore ReloadRestriction { get; } = new(1, 1);
    public Group? CurrentGroup { get; private set; }
    private GroupContext? _context;

    public Person? CurrentPerson { get; private set; }

    // Loads group from database
    // Does not reload if the slug is the same
    public async Task LoadGroup(string slug)
    {
        if (CurrentGroup?.GroupSlug == slug)
            return;

        await ForceLoadGroup(slug);
    }

    private async Task ForceLoadGroup(string slug)
    {
        if (CurrentGroup != null)
        {
            autoreloadService.GetHandlerForGroup(CurrentGroup).OnGroupUpdated -= OnGroupUpdated;
        }

        if (_context != null)
            await _context.DisposeAsync();

        CurrentPerson = null;

        // Create a new Context to not have issues with other groups
        // still loaded
        _context = await dbFactory.CreateDbContextAsync();
        CurrentGroup = await _context
            .Groups.Include(group => group.Orders)
            .ThenInclude(order => order.Person)
            .Include(group => group.Persons)
            .ThenInclude(person => person.Payments)
            .SingleOrDefaultAsync(c => c.GroupSlug == slug);
        if (CurrentGroup != null)
        {
            autoreloadService.GetHandlerForGroup(CurrentGroup).OnGroupUpdated += OnGroupUpdated;
            Task.Run(() =>
            {
                OnGroupReload?.Invoke(this, EventArgs.Empty);
            });
        }
    }

    private void OnGroupUpdated(object? sender, EventArgs e)
    {
        ReloadRestriction.WaitOne();
        ReloadGroup().Wait();
        ReloadRestriction.Release();
    }

    public async Task ReloadGroup()
    {
        if (_context == null)
            return;
        if (CurrentGroup == null)
            return;

        await ForceLoadGroup(CurrentGroup.GroupSlug!);
    }

    public async Task ReloadOrder(Order order)
    {
        if (_context == null)
            return;

        await _context.Entry(order).ReloadAsync();
    }

    public void DeleteOrder(Order order)
    {
        _context?.Orders.Remove(order);
    }

    public void DeletePayment(Payment payment)
    {
        _context?.Payments.Remove(payment);
    }

    public void AddPerson(Person person)
    {
        if (CurrentGroup == null)
            return;
        person.Group = CurrentGroup;
        _context?.Add(person);
    }

    public Person? SetCurrentPersonId(int id)
    {
        CurrentPerson = CurrentGroup?.Persons.SingleOrDefault(p => p.Id == id);
        return CurrentPerson;
    }

    public void AddOrder(Order order, Person person)
    {
        if (CurrentGroup == null)
            return;
        order.Group = CurrentGroup;
        order.Person = person;
        if (CurrentGroup != person.Group)
            throw new InvalidDataException("Person is not in the group");
        order.AddedToCart = false;
        if (CurrentGroup.PaymentType == PaymentType.NoPrices)
        {
            order.Price = 0;
        }

        _context?.Add(order);
    }

    public void AddPayment(Payment payment, Person person)
    {
        if (CurrentGroup == null)
            return;
        payment.Person = person;
        if (CurrentGroup.PaymentType != PaymentType.Pay)
        {
            throw new InvalidDataException("Cant add Payment to Group without Payments");
        }

        _context?.Add(payment);
    }

    public async Task Save()
    {
        if (_context == null)
            return;

        await _context.SaveChangesAsync();

        Task.Run(() =>
        {
            autoreloadService.GetHandlerForGroup(CurrentGroup!).Call();
        });
    }

    public bool IsOrderingClosed()
    {
        return DateTime.Now > CurrentGroup!.ClosingTime;
    }

    public void Dispose()
    {
        _context?.Dispose();
        if (CurrentGroup != null)
        {
            autoreloadService.GetHandlerForGroup(CurrentGroup).OnGroupUpdated -= OnGroupUpdated;
        }
    }
}
