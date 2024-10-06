using System.Diagnostics;

namespace GroupOrder.Data;
using Microsoft.EntityFrameworkCore;

public class GroupContext : DbContext
{
    public DbSet<Group> Groups { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Person> Persons { get; set; }
    
    public DbSet<VanityURL> VanityUrls { get; set; }
    private string DbPath { get; }

    public GroupContext()
    {
        System.IO.Directory.CreateDirectory(System.IO.Path.Join(System.IO.Directory.GetCurrentDirectory(), "var/"));
        DbPath = System.IO.Path.Join(System.IO.Directory.GetCurrentDirectory(), "var/groups.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={DbPath}");
    }
}