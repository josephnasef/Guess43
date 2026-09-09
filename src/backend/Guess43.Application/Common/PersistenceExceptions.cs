namespace Guess43.Application.Common;

/// <summary>
/// Thrown by the persistence layer when an optimistic-concurrency conflict occurs,
/// so application handlers can translate it into a stable Result error.
/// </summary>
public sealed class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Thrown by the persistence layer when a unique constraint is violated
/// (e.g. duplicate email or a second active game under concurrency).
/// </summary>
public sealed class UniqueConstraintConflictException : Exception
{
    public UniqueConstraintConflictException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
