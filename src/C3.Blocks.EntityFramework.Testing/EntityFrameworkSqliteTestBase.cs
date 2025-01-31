using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace C3.Blocks.Repository.Testing.Sqlite;

/// <summary>
/// Provides a base class for running tests with an in-memory SQLite database.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
public abstract class EntityFrameworkSqliteTestBase<TDbContext> : EntityFrameworkTestBase<TDbContext>
    where TDbContext : DbContext
{
    /// <inheritdoc/>
    protected override DbConnection CreateSqlConnection() => new SqliteConnection("Datasource=:memory:");

    /// <inheritdoc/>
    protected override DbContextOptions<TDbContext> MakeDbContextOptions(DbContextOptionsBuilder<TDbContext> dbContextOptionsBuilder, DbConnection connection) =>
        dbContextOptionsBuilder
            .UseSqlite(connection)
            .Options;
}
