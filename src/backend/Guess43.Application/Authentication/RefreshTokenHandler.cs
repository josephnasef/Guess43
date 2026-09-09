using Guess43.Application.Abstractions;
using Guess43.Application.Authentication.Contracts;
using Guess43.Application.Common;
using Guess43.Domain.Users;
using Microsoft.Extensions.Logging;

namespace Guess43.Application.Authentication;

/// <summary>Rotates a refresh token, detecting reuse of a previously rotated token.</summary>
public sealed class RefreshTokenHandler(
    IUserRepository users,
    IRefreshTokenRepository refreshTokens,
    IAccessTokenService accessTokens,
    IRefreshTokenService refreshTokenService,
    IUnitOfWork unitOfWork,
    IClock clock,
    ILogger<RefreshTokenHandler> logger)
{
    public async Task<Result<AuthenticationResult>> HandleAsync(
        string? rawRefreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawRefreshToken))
        {
            return ApplicationErrors.InvalidRefreshToken;
        }

        var hash = refreshTokenService.Hash(rawRefreshToken);
        var stored = await refreshTokens.GetByHashAsync(hash, cancellationToken);
        if (stored is null)
        {
            return ApplicationErrors.InvalidRefreshToken;
        }

        var now = clock.UtcNow;

        // Reuse detection: a token that was already rotated is being presented again.
        if (stored.ReplacedByTokenHash is not null)
        {
            logger.LogWarning("Refresh token reuse detected for user {UserId}; revoking token family", stored.UserId);
            await refreshTokens.RevokeAllForUserAsync(stored.UserId, now, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return ApplicationErrors.InvalidRefreshToken;
        }

        if (!stored.IsActive(now))
        {
            return ApplicationErrors.InvalidRefreshToken;
        }

        var user = await users.GetByIdAsync(stored.UserId, cancellationToken);
        if (user is null)
        {
            return ApplicationErrors.InvalidRefreshToken;
        }

        var newRefresh = refreshTokenService.Issue();
        stored.RevokeAndReplace(newRefresh.TokenHash, now);
        refreshTokens.Add(RefreshToken.Issue(user.Id, newRefresh.TokenHash, now, newRefresh.ExpiresAtUtc));

        var access = accessTokens.Issue(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthenticationResult(
            access.Value,
            access.ExpiresAtUtc,
            newRefresh.RawValue,
            newRefresh.ExpiresAtUtc,
            Users.UserMapping.ToProfile(user));
    }
}
