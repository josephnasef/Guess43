namespace Guess43.Domain.Games;

/// <summary>
/// A server-authoritative "Guess the Number" session. The secret number is set
/// once at creation and is never exposed outside the domain/persistence boundary.
/// </summary>
public sealed class GameSession
{
    // Parameterless ctor for EF Core materialisation.
    private GameSession()
    {
    }

    private GameSession(Guid id, Guid userId, int targetNumber, DateTime startedAtUtc)
    {
        if (!GameLimits.IsInRange(targetNumber))
        {
            throw new ArgumentOutOfRangeException(nameof(targetNumber));
        }

        Id = id;
        UserId = userId;
        TargetNumber = targetNumber;
        GuessCount = 0;
        Status = GameStatus.Active;
        StartedAtUtc = startedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    /// <summary>The secret number. Never expose through DTOs, logs, or the API.</summary>
    public int TargetNumber { get; private set; }

    public int GuessCount { get; private set; }

    public GameStatus Status { get; private set; }

    public DateTime StartedAtUtc { get; private set; }

    public DateTime? CompletedAtUtc { get; private set; }

    /// <summary>Optimistic-concurrency token (mapped to PostgreSQL xmin).</summary>
    public uint Version { get; private set; }

    public bool IsActive => Status == GameStatus.Active;

    /// <summary>
    /// Starts a new active session for a user with a pre-generated secret number.
    /// The secret must be produced by a cryptographic RNG in the range 1-43.
    /// </summary>
    public static GameSession Start(Guid userId, int targetNumber, DateTime startedAtUtc)
        => new(Guid.NewGuid(), userId, targetNumber, startedAtUtc);

    /// <summary>
    /// Submits a guess. Increments the accepted-guess counter exactly once and
    /// returns the outcome. Completing the game is atomic with the final guess.
    /// </summary>
    /// <exception cref="GameAlreadyCompletedException">The game is already completed.</exception>
    /// <exception cref="GuessOutOfRangeException">The guess is outside 1-43.</exception>
    public GuessOutcome SubmitGuess(int guess, DateTime nowUtc)
    {
        if (Status == GameStatus.Completed)
        {
            throw new GameAlreadyCompletedException();
        }

        if (!GameLimits.IsInRange(guess))
        {
            throw new GuessOutOfRangeException(guess);
        }

        GuessCount++;

        if (guess == TargetNumber)
        {
            Status = GameStatus.Completed;
            CompletedAtUtc = nowUtc;
            return GuessOutcome.Correct;
        }

        return guess < TargetNumber ? GuessOutcome.Higher : GuessOutcome.Lower;
    }
}
