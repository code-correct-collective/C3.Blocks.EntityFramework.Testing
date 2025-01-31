using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

namespace C3.Blocks.Repository.Testing.Sqlite;

/// <summary>
/// Base class for Entity Framework tests using SQLite.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
public abstract class EntityFrameworkTestBase<TDbContext>
    where TDbContext : DbContext
{
    /// <summary>
    /// Creates a new SQL connection.
    /// </summary>
    /// <returns>A new instance of <see cref="IDbConnection"/>.</returns>
    protected abstract DbConnection CreateSqlConnection();

    /// <summary>
    /// Creates the DbContext options for the specified database context.
    /// </summary>
    /// <param name="dbContextOptionsBuilder">The DbContext options to configure.</param>
    /// <param name="connection">The database connection to use.</param>
    /// <returns>A new instance of <see cref="DbContextOptions{TDbContext}"/>.</returns>
    protected abstract DbContextOptions<TDbContext> MakeDbContextOptions(DbContextOptionsBuilder<TDbContext> dbContextOptionsBuilder, DbConnection connection);


    /// <summary>
    /// Runs a test asynchronously with the specified runner and optional setup actions.
    /// </summary>
    /// <param name="operationAsync">The function to run the test.</param>
    /// <param name="setupAsync">The optional setup function to prepare the test environment.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected virtual async Task RunTestAsync(
        Func<TDbContext, CancellationToken, Task> operationAsync,
        Func<TDbContext, CancellationToken, Task>? setupAsync = null,
        CancellationToken cancellationToken = default)
    {
        operationAsync = operationAsync ?? throw new ArgumentNullException(nameof(operationAsync));
        using var connection = this.CreateSqlConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var options = this.MakeDbContextOptions(new DbContextOptionsBuilder<TDbContext>(), connection);

            using (var context = MakeDbContext(options))
            {
                await context.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);
            }

            if (setupAsync != null)
            {
                using (var context = MakeDbContext(options))
                {
                    await setupAsync(context, cancellationToken).ConfigureAwait(false);
                }
            }

            using (var context = MakeDbContext(options))
            {
                await operationAsync(context, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception x)
        {
            Debug.WriteLine(x.ToString());
            throw;
        }
        finally
        {
            await connection.CloseAsync().ConfigureAwait(false);
        }
    }

    private static TDbContext MakeDbContext(DbContextOptions<TDbContext> options)
    {
        var context = Activator.CreateInstance(typeof(TDbContext), options) as TDbContext;

        return context!;
    }
}
