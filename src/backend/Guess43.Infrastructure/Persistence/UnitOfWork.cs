using Guess43.Application.Abstractions;
using Guess43.Application.Common;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Guess43.Infrastructure.Persistence;

/// <summary>
/// Defines the use-case transaction boundary and translates provider-specific
/// persistence failures into stable Application exceptions.
/// </summary>
public sealed class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    private const string UniqueViolationSqlState = "23505";

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyConflictException("The resource was modified concurrently.", ex);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: UniqueViolationSqlState })
        {
            throw new UniqueConstraintConflictException("A unique constraint was violated.", ex);
        }
    }

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            await operation(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }
}
