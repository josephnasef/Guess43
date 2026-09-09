namespace Guess43.Application.Abstractions;

/// <summary>Provides the current UTC time. Injectable so tests are deterministic.</summary>
public interface IClock
{
    DateTime UtcNow { get; }
}

/// <summary>Produces the secret number for a new game using a secure RNG in tests and production.</summary>
public interface ISecretNumberGenerator
{
    /// <summary>Returns an integer in the inclusive range 1-43.</summary>
    int Next();
}

/// <summary>Defines the use-case transaction boundary. Repositories stage changes only.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Executes work inside a single database transaction (for multi-step operations).</summary>
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default);
}
