namespace Guess43.Domain.Games;

/// <summary>Result of an accepted guess relative to the secret number.</summary>
public enum GuessOutcome
{
    /// <summary>The secret number is higher than the guess.</summary>
    Higher = 1,

    /// <summary>The secret number is lower than the guess.</summary>
    Lower = 2,

    /// <summary>The guess matches the secret number.</summary>
    Correct = 3,
}
