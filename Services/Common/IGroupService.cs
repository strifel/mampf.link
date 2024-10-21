namespace GroupOrder.Services.Common;

using Data;

public interface IGroupService {

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

    // Adds a specific person to the group
    // Needs a save afterwards
    public void AddPerson(Person person);
    
    // Adds a specific order to the group
    // order is from person person
    // Needs a save afterwards
    public void AddOrder(Order order, Person person);
    
    // Saves the changes to the group
    public Task Save();

}
