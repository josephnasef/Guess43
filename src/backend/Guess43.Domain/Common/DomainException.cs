namespace Guess43.Domain.Common;

/// <summary>
/// Base type for exceptions that represent a violated domain invariant.
/// These are defensive guards; expected business outcomes are represented
/// with the Application-layer Result type, not exceptions.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message)
        : base(message)
    {
    }
}
