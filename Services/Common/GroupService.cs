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
    public Group? Group { get; private set; }
    private GroupContext? _context;

    public bool NotFound { get; private set; }
    public bool Loading { get; private set; }

    // Loads group from database
    // Does not reload if the slug is the same
    public async Task LoadGroup(string slug)
    {
        if (Group != null && slug == Group.GroupSlug)
            return;

        await ForceLoadGroup(slug);
    }

    private async Task ForceLoadGroup(String slug)
    {
        Loading = true;

        if (Group != null)
        {
            autoreloadService.getHandlerForGroup(Group).OnGroupUpdated -= OnGroupUpdated;
        }

        if (_context != null)
            await _context.DisposeAsync();

        // Create a new Context to not have issues with other groups
        // still loaded
        _context = dbFactory.CreateDbContext();
        Group = await _context
            .Groups.Include(group => group.Orders)
            .ThenInclude(order => order.Person)
            .Include(group => group.Persons)
            .ThenInclude(person => person.Payments)
            .SingleOrDefaultAsync(c => c.GroupSlug == slug);
        if (Group == null)
        {
            NotFound = true;
        }
        else
        {
            autoreloadService.getHandlerForGroup(Group).OnGroupUpdated += OnGroupUpdated;
            Task.Run(() =>
            {
                OnGroupReload?.Invoke(this, EventArgs.Empty);
            });
        }
        Loading = false;
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
        if (Group == null)
            return;

        await ForceLoadGroup(Group.GroupSlug!);
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
        if (Group == null)
            return;
        person.Group = Group;
        _context?.Add(person);
    }

    public Person? GetPersonByID(int id)
    {
        return Group?.Persons.FirstOrDefault(p => p.Id == id);
    }

    public void AddOrder(Order order, Person person)
    {
        if (Group == null)
            return;
        order.Group = Group;
        order.Person = person;
        if (Group != person.Group)
            throw new InvalidDataException("Person is not in the group");
        order.AddedToCart = false;
        if (Group.PaymentType == PaymentType.NO_PRICES)
        {
            order.Price = 0;
        }
        _context?.Add(order);
    }

    public void AddPayment(Payment payment, Person person)
    {
        if (Group == null)
            return;
        payment.Person = person;
        if (Group.PaymentType != PaymentType.PAY)
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
            autoreloadService.getHandlerForGroup(Group!).Call();
        });
    }

    public bool IsOrderingClosed()
    {
        return DateTime.Now > Group!.ClosingTime;
    }

    public void Dispose()
    {
        _context?.Dispose();
        if (Group != null)
        {
            autoreloadService.getHandlerForGroup(Group).OnGroupUpdated -= OnGroupUpdated;
        }
    }
}
