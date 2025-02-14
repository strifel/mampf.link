namespace GroupOrder.Services.Common;

using Data;
using Microsoft.EntityFrameworkCore;

public class GroupService(
    ILogger<GroupService> logger,
    IDbContextFactory<GroupContext> dbFactory,
    IGroupAutoreloadService autoreloadService
) : IGroupService, IDisposable
{
    public event EventHandler? OnGroupReload;

    public Semaphore ReloadRestriction { get; } = new(1, 1);
    public Group? CurrentGroup { get; private set; }
    private GroupContext? _dbContext;

    public Person? CurrentPerson { get; private set; }

    /// <summary>
    /// Loads group from database
    /// Does not reload if the slug is the samey
    /// </summary>
    /// <param name="slug"></param>
    public async Task LoadGroup(string slug)
    {
        if (CurrentGroup?.GroupSlug == slug)
        {
            logger.LogDebug("Group '{Slug}' already loaded, doing nothing", slug);
            return;
        }

        await ForceLoadGroup(slug);
    }

    private async Task ForceLoadGroup(string slug)
    {
        if (CurrentGroup != null)
        {
            autoreloadService.GetHandlerForGroup(CurrentGroup).OnGroupUpdated -= OnGroupUpdated;
        }

        if (_dbContext != null)
            await _dbContext.DisposeAsync();

        // Create a new Context to not have issues with other groups
        // still loaded
        _dbContext = await dbFactory.CreateDbContextAsync();
        CurrentGroup = await _dbContext
            .Groups.Where(c => c.GroupSlug == slug)
            .Include(group => group.Orders)
            .ThenInclude(order => order.Person)
            .Include(group => group.Persons)
            .ThenInclude(person => person.Payments)
            .FirstOrDefaultAsync();

        if (CurrentPerson != null)
        {
            CurrentPerson = CurrentGroup?.Persons.SingleOrDefault(p => p.Id == CurrentPerson.Id);
        }

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
        if (_dbContext == null)
            return;
        if (CurrentGroup == null)
            return;

        await ForceLoadGroup(CurrentGroup.GroupSlug!);
    }

    public void DeleteOrder(Order order)
    {
        _dbContext?.Orders.Remove(order);
    }

    public void DeletePayment(Payment payment)
    {
        _dbContext?.Payments.Remove(payment);
    }

    public async Task CreateNewPerson(string name)
    {
        if (CurrentGroup == null)
            return;

        ReloadRestriction.WaitOne();

        Person person = new Person { Group = CurrentGroup, Name = name };

        person.Group = CurrentGroup;
        _dbContext?.Add(person);

        await Save();

        CurrentPerson = person;

        ReloadRestriction.Release();
    }

    public void SetCurrentPersonId(int id)
    {
        CurrentPerson = CurrentGroup?.Persons.SingleOrDefault(p => p.Id == id);
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

        _dbContext?.Add(order);
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

        _dbContext?.Add(payment);
    }

    public async Task Save()
    {
        if (_dbContext == null)
            return;

        await _dbContext.SaveChangesAsync();

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
        _dbContext?.Dispose();
        if (CurrentGroup != null)
        {
            autoreloadService.GetHandlerForGroup(CurrentGroup).OnGroupUpdated -= OnGroupUpdated;
        }
    }
}
