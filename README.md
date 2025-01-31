# C3.Blocks.EntityFramework.Testing

This library is a simple library to assist with testing EntityFramework libraries against a Sqlite database
or some other database type.

The library attempts to be unopinionated in that you can use the [EntityFrameworkSqliteTestBase](./src/C3.Blocks.EntityFramework.Testing/EntityFrameworkSqliteTestBase.cs)
to test against an in-memory sqlite database. You can also create a new subclass of [EntityFrameworkTestBase](./src/C3.Blocks.EntityFramework.Testing/EntityFrameworkTestBase.cs)
to connect to the database engine of your choice.

[![Open in DevPod!](https://devpod.sh/assets/open-in-devpod.svg)](https://devpod.sh/open#https://github.com/code-correct-collective/C3.Blocks.EntityFramework.Testing)

## Connecting to a custom database engine

1. Create a subclass of `EntityFrameworkTestBase` (See [EntityFrameworkSqliteTestBase](./src/C3.Blocks.EntityFramework.Testing/EntityFrameworkSqliteTestBase.cs) as an example):
   ```csharp
    public abstract class EntityFrameworkSqliteTestBase<TDbContext> : EntityFrameworkTestBase<TDbContext>
        where TDbContext : DbContext
    {
        protected override DbConnection CreateSqlConnection() => new SqliteConnection("Datasource=:memory:");

        protected override DbContextOptions<TDbContext> MakeDbContextOptions(
            DbContextOptionsBuilder<TDbContext> dbContextOptionsBuilder,
            DbConnection connection) =>
                dbContextOptionsBuilder
                   .UseSqlite(connection)
                   .Options;
    }
   ```
2. Modify the above code to create your own `DbConnection` and to return the correct `DbConnectionOptions<TDbContext>`

## Create Unit Tests, Setting up the Database before each Test
The idea is that it is quick to create a new database and migrate the changes to it before each test.
This way the tests can be repeatable on each run.

See [TestDbContextTests](./tests/C3.Blocks.EntityFramework.Testing.Tests/TestDbContextTests.cs) for an example how create a test.

```csharp
public class TestDbContextTests : EntityFrameworkSqliteTestBase<TestDbContext>
{
    [Fact]
    public async Task RunTestMethodTest()
    {
        // Arrange
        var d1 = new TestDomainObject { Name = "d1" };
        var d2 = new TestDomainObject { Name = "d2" };
        var d3 = new TestDomainObject { Name = "d3" };

        // Act, Assert
        await this.RunTestAsync(
            async Task (context, cancellationToken) =>
            {
                var items = await context.TestDomainObject.ToListAsync(cancellationToken);

                // Assert
                Assert.Equal(3, items.Count);
                Assert.NotNull(items.FirstOrDefault(i => i.Name == "d2"));
            },
            async Task (context, cancellationToken) =>
            {
                context.AddRange([d3, d2, d1]);
                await context.SaveChangesAsync(cancellationToken);
            }
        );
    }
    // Other test remove fro brevity
}
```

### Basic Steps
1. The test class should derive from `EntityFrameworkSqliteTest<TDBContext>` 
where `TDbContext` is the `Dbcontext` type specific to your project.
2. In your test method, call the `RunTest` method, passing in the two functions for your test, the first function for the operation to run that is under test and the second function to setup the data required for the operation function.
