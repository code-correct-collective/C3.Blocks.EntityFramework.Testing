using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

namespace C3.Blocks.EntityFramework.Testing;

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
    /// <exception cref="ArgumentNullException"><c>operationAsync</c> must not be null.</exception>
    protected virtual async Task RunTestAsync(
        Func<TDbContext, CancellationToken, Task> operationAsync,
        Func<TDbContext, CancellationToken, Task>? setupAsync = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operationAsync, nameof(operationAsync));
        using var connection = this.CreateSqlConnection();

        try
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            var options = this.MakeDbContextOptions(new DbContextOptionsBuilder<TDbContext>(), connection);

            await CreateDbSchema(options, cancellationToken).ConfigureAwait(false);

            if (setupAsync != null)
            {
                using var setupContext = MakeDbContext(options);
                await setupAsync(setupContext, cancellationToken).ConfigureAwait(false);
            }

            using var context = MakeDbContext(options);
            await operationAsync(context, cancellationToken).ConfigureAwait(false);
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

    private static async Task CreateDbSchema(DbContextOptions<TDbContext> options, CancellationToken cancellationToken)
    {
        using var createSchema = MakeDbContext(options);
        await createSchema.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);
    }

    private static TDbContext MakeDbContext(DbContextOptions<TDbContext> options)
    {
        var context = Activator.CreateInstance(typeof(TDbContext), options) as TDbContext;

        return context!;
    }
}
