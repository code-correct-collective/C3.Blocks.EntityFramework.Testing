using Microsoft.EntityFrameworkCore;

namespace C3.Blocks.EntityFramework.Testing.Tests;

public class TestDbContext : DbContext
{
    public DbSet<TestDomainObject> TestDomainObject { get; set; }

    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
        this.TestDomainObject = this.Set<TestDomainObject>();
    }
}
