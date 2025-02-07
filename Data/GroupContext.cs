namespace GroupOrder.Data;

using Microsoft.EntityFrameworkCore;

public class GroupContext : DbContext
{
    public DbSet<Group> Groups { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Person> Persons { get; set; }

    public DbSet<Payment> Payments { get; set; }
    public DbSet<VanityURL> VanityUrls { get; set; }
    private string DbPath { get; }

    public GroupContext()
    {
        Directory.CreateDirectory(
            Path.Join(Directory.GetCurrentDirectory(), "var/")
        );
        DbPath = Path.Join(Directory.GetCurrentDirectory(), "var/groups.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={DbPath}");
    }
}
