namespace Guess43.Domain.Games;

/// <summary>
/// Inclusive bounds for the secret number and accepted guesses.
/// Centralised so the range is never a magic number.
/// </summary>
public static class GameLimits
{
    public const int MinNumber = 1;
    public const int MaxNumber = 43;

    /// <summary>Exclusive upper bound for RandomNumberGenerator.GetInt32.</summary>
    public const int RandomExclusiveUpperBound = MaxNumber + 1;

    public static bool IsInRange(int value) => value is >= MinNumber and <= MaxNumber;
}
