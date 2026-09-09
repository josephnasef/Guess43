namespace Guess43.Domain.Users;

/// <summary>
/// A rotating refresh token. Only the cryptographic hash of the token is stored;
/// the plaintext is never persisted or logged. Supports rotation and reuse detection.
/// </summary>
public sealed class RefreshToken
{
    private RefreshToken()
    {
        TokenHash = null!;
    }

    private RefreshToken(
        Guid id,
        Guid userId,
        string tokenHash,
        DateTime createdAtUtc,
        DateTime expiresAtUtc)
    {
        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    /// <summary>SHA-256 hash of the refresh token. Never the plaintext.</summary>
    public string TokenHash { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime ExpiresAtUtc { get; private set; }

    public DateTime? RevokedAtUtc { get; private set; }

    /// <summary>Hash of the token that replaced this one during rotation.</summary>
    public string? ReplacedByTokenHash { get; private set; }

    public bool IsRevoked => RevokedAtUtc is not null;

    public bool IsExpired(DateTime nowUtc) => nowUtc >= ExpiresAtUtc;

    public bool IsActive(DateTime nowUtc) => !IsRevoked && !IsExpired(nowUtc);

    public static RefreshToken Issue(
        Guid userId,
        string tokenHash,
        DateTime createdAtUtc,
        DateTime expiresAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);
        return new RefreshToken(Guid.NewGuid(), userId, tokenHash, createdAtUtc, expiresAtUtc);
    }

    public void Revoke(DateTime nowUtc)
    {
        RevokedAtUtc ??= nowUtc;
    }

    /// <summary>Revokes this token and links it to its replacement during rotation.</summary>
    public void RevokeAndReplace(string replacementTokenHash, DateTime nowUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(replacementTokenHash);
        Revoke(nowUtc);
        ReplacedByTokenHash = replacementTokenHash;
    }
}
