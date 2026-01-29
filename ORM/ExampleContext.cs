using ORM.Core;
using ORM.Models;

namespace ORM;

public class ExampleContext : DbContext
{
    public DbSet<User> Users => Set<User>();

    public ExampleContext(string connectionString) : base(connectionString)
    {
    }
}
