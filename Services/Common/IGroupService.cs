namespace GroupOrder.Services.Common;

using Data;

public interface IGroupService {

    // EventHandler that emits an event when the group is reloaded
    public event EventHandler? OnGroupReload;
    
    // Semaphore to restrict reloads (e.g. while editing)
    public Semaphore ReloadRestriction { get; }

    // Returns the group that is currently loaded
    public Group? Group { get; }
    
    // Returns if the group is currently loading
    public bool Loading { get; }
    
    // Returns if the group was not found
    public bool NotFound { get; }

    // Loads group by slug
    // Does not reload if the slug is the same
    public Task LoadGroup(String slug);

    // Reload current loaded group
    public Task ReloadGroup();
    
    // Reload only a specific order
    public Task ReloadOrder(Order order);

    // Delete specific order from group
    // Needs a save afterwards
    public void DeleteOrder(Order order);

    // Delete specific payment from group
    // Needs a save afterwards
    public void DeletePayment(Payment payment);

    // Adds a specific person to the group
    // Needs a save afterwards
    public void AddPerson(Person person);
    
    // Returns the current instance of a specific person by ID
    // ReSharper disable once InconsistentNaming
    public Person? GetPersonByID(int id);
    
    // Adds a specific order to the group
    // order is from person person
    // Needs a save afterwards
    public void AddOrder(Order order, Person person);
    
    // Adds a specific payment to the group
    // order is from person person
    // Needs a save afterwards
    public void AddPayment(Payment payment, Person person);
    
    // Saves the changes to the group
    public Task Save();

}
