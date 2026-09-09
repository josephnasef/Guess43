using Guess43.Application.Abstractions;
using Guess43.Application.Common;
using Microsoft.Extensions.Logging;

namespace Guess43.Application.Users;

/// <summary>
/// Deletes the current account. Revokes refresh tokens and removes dependent game
/// data inside a single transaction so the removal is atomic.
/// </summary>
public sealed class DeleteAccountHandler(
    IUserRepository users,
    IGameSessionRepository games,
    IRefreshTokenRepository refreshTokens,
    IUnitOfWork unitOfWork,
    ILogger<DeleteAccountHandler> logger)
{
    public async Task<Result> HandleAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return ApplicationErrors.UserNotFound;
        }

        await unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                await refreshTokens.RemoveAllForUserAsync(userId, ct);
                await games.RemoveOwnedForUserAsync(userId, ct);
                users.Remove(user);
                await unitOfWork.SaveChangesAsync(ct);
            },
            cancellationToken);

        logger.LogInformation("Account {UserId} deleted with dependent data", userId);
        return Result.Success();
    }
}
