using Guess43.Domain.Users;

namespace Guess43.Application.Abstractions;

/// <summary>Persistence operations for refresh tokens (stored as hashes only).</summary>
public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    void Add(RefreshToken token);

    /// <summary>Revokes every active token for a user (logout / account deletion).</summary>
    Task RevokeAllForUserAsync(Guid userId, DateTime nowUtc, CancellationToken cancellationToken = default);

    Task RemoveAllForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
