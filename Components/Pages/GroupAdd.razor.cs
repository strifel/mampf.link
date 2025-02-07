using GroupOrder.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;

namespace GroupOrder.Components.Pages;

public partial class GroupAdd
{
    private EditContext? editContext;
    private ValidationMessageStore? messageStore;
    private Order? Order { get; set; }
    private Decimal? Price { get; set; }
    private String? Name { get; set; }
    private bool Saved { get; set; } = false;

    [Parameter]
    public string? GroupSlug { get; set; }

    protected override void OnInitialized()
    {
        Order ??= new();
        editContext = new(Order);
        editContext.OnValidationRequested += HandleValidationRequested;
        messageStore = new(editContext);
    }

    private void reset()
    {
        Order = new();
        Price = null;
        Name = null;
        editContext = new(Order);
        editContext.OnValidationRequested += HandleValidationRequested;
        messageStore = new(editContext);
        Saved = false;
    }

    private async void Submit()
    {
        Order!.Price = (int)(Price! * 100);

        using var context = DbFactory.CreateDbContext();

        Data.Group? group = await context.Groups.SingleOrDefaultAsync(c =>
            c.GroupSlug == GroupSlug
        );

        if (group is null)
        {
            return;
        }

        Person? person = await context.Persons.FirstOrDefaultAsync(p =>
            p.Group == group && p.Name == Name
        );

        if (person is null)
        {
            person = new Person();
            person.Group = group;
            person.Name = Name;
            context.Add(person);
        }

        Order!.AddedToCart = false;
        Order!.Group = group;
        Order!.Person = person;
        context.Add(Order!);
        context.SaveChanges();
        Saved = true;
        await gs.ReloadGroup();
    }

    private void HandleValidationRequested(object? sender, ValidationRequestedEventArgs args)
    {
        messageStore?.Clear();

        if (Name == null || Name!.Length == 0 || Name!.Length > 100)
        {
            messageStore?.Add(() => Order!, "You must enter a Name between 1 and 100 chars.");
        }

        if (Order!.Food == null || Order!.Food!.Length == 0 || Order!.Food!.Length > 100)
        {
            messageStore?.Add(() => Order, "You must enter a Food Choice between 1 and 100 chars.");
        }

        if (Price == null | Price < 0)
        {
            messageStore?.Add(() => Order, "Price must be greater than 0.");
        }
    }

    public void Dispose()
    {
        if (editContext is not null)
        {
            editContext.OnValidationRequested -= HandleValidationRequested;
        }
    }
}
