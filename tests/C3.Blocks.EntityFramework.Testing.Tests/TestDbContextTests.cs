using C3.Blocks.Repository.Testing.Sqlite.Tests.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace C3.Blocks.Repository.Testing.Sqlite.Tests;

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

    [Fact]
    public async Task RunTestMethodTestWithNullRunner()
    {
        // Arrange, Act, Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => this.RunTestAsync(null!));
    }

    [Fact]
    public async Task RunTestMethodWhenRunnerThrowsException()
    {
        // Arrange, Act, Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => this.RunTestAsync((context, c) => throw new InvalidOperationException("Failed")));
    }
}
