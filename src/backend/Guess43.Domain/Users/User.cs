namespace Guess43.Domain.Users;

/// <summary>
/// A registered player. Owns the single persisted personal-best field
/// (<see cref="BestGuessCount"/>) required by the assignment.
/// </summary>
public sealed class User
{
    // Parameterless ctor for EF Core materialisation.
    private User()
    {
        Email = null!;
        NormalizedEmail = null!;
        DisplayName = null!;
        PasswordHash = null!;
    }

    private User(
        Guid id,
        string email,
        string normalizedEmail,
        string displayName,
        string passwordHash,
        DateTime createdAtUtc)
    {
        Id = id;
        Email = email;
        NormalizedEmail = normalizedEmail;
        DisplayName = displayName;
        PasswordHash = passwordHash;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public string Email { get; private set; }

    public string NormalizedEmail { get; private set; }

    public string DisplayName { get; private set; }

    public string PasswordHash { get; private set; }

    /// <summary>Lowest number of guesses across completed games; null until the first win.</summary>
    public int? BestGuessCount { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public static User Register(
        string email,
        string displayName,
        string passwordHash,
        DateTime createdAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        var trimmedEmail = email.Trim();
        return new User(
            Guid.NewGuid(),
            trimmedEmail,
            NormalizeEmail(trimmedEmail),
            displayName.Trim(),
            passwordHash,
            createdAtUtc);
    }

    /// <summary>Normalises an email for case-insensitive uniqueness.</summary>
    public static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();

    /// <summary>
    /// Records the result of a completed game and improves the personal best when
    /// the new guess count is smaller. Returns true when a new record was set.
    /// </summary>
    public bool TryImprovePersonalBest(int guessCount, DateTime nowUtc)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(guessCount);

        if (BestGuessCount is null || guessCount < BestGuessCount.Value)
        {
            BestGuessCount = guessCount;
            UpdatedAtUtc = nowUtc;
            return true;
        }

        return false;
    }

    public void UpdateProfile(string displayName, DateTime nowUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        DisplayName = displayName.Trim();
        UpdatedAtUtc = nowUtc;
    }

    public void SetPasswordHash(string passwordHash, DateTime nowUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        PasswordHash = passwordHash;
        UpdatedAtUtc = nowUtc;
    }
}
