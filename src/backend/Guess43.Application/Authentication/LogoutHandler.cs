using Guess43.Application.Abstractions;
using Guess43.Application.Common;
using Microsoft.Extensions.Logging;

namespace Guess43.Application.Authentication;

/// <summary>Revokes the presented refresh token's family so the session cannot be resumed.</summary>
public sealed class LogoutHandler(
    IRefreshTokenRepository refreshTokens,
    IRefreshTokenService refreshTokenService,
    IUnitOfWork unitOfWork,
    IClock clock,
    ILogger<LogoutHandler> logger)
{
    public async Task<Result> HandleAsync(string? rawRefreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawRefreshToken))
        {
            return Result.Success();
        }

        var hash = refreshTokenService.Hash(rawRefreshToken);
        var stored = await refreshTokens.GetByHashAsync(hash, cancellationToken);
        if (stored is not null)
        {
            await refreshTokens.RevokeAllForUserAsync(stored.UserId, clock.UtcNow, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogInformation("User {UserId} logged out; token family revoked", stored.UserId);
        }

        return Result.Success();
    }
}
