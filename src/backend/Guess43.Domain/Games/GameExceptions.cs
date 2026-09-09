using Guess43.Domain.Common;

namespace Guess43.Domain.Games;

/// <summary>Raised when a guess falls outside the accepted range.</summary>
public sealed class GuessOutOfRangeException : DomainException
{
    public GuessOutOfRangeException(int guess)
        : base($"Guess {guess} is outside the accepted range {GameLimits.MinNumber}-{GameLimits.MaxNumber}.")
    {
    }
}

/// <summary>Raised when a guess is submitted to an already-completed game.</summary>
public sealed class GameAlreadyCompletedException : DomainException
{
    public GameAlreadyCompletedException()
        : base("Guesses cannot be submitted to a completed game.")
    {
    }
}
