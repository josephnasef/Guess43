using Guess43.Application.Abstractions;
using Guess43.Application.Common;
using Guess43.Application.Games.Contracts;
using Guess43.Domain.Games;
using Microsoft.Extensions.Logging;

namespace Guess43.Application.Games;

/// <summary>
/// Starts a new game or returns the caller's existing active game so that a
/// repeated start is idempotent and never creates a second active session.
/// </summary>
public sealed class StartGameHandler(
    IGameSessionRepository games,
    ISecretNumberGenerator secretNumbers,
    IUnitOfWork unitOfWork,
    IClock clock,
    ILogger<StartGameHandler> logger)
{
    public async Task<Result<GameSummaryResponse>> HandleAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var existing = await games.GetActiveForUserAsync(userId, cancellationToken);
        if (existing is not null)
        {
            return existing.ToSummary();
        }

        var session = GameSession.Start(userId, secretNumbers.Next(), clock.UtcNow);
        games.Add(session);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is ConcurrencyConflictException or UniqueConstraintConflictException)
        {
            // A concurrent start won the race; return the now-existing active game.
            var active = await games.GetActiveForUserAsync(userId, cancellationToken);
            if (active is not null)
            {
                return active.ToSummary();
            }

            return ApplicationErrors.ConcurrencyConflict;
        }

        logger.LogInformation("Game {GameId} started for user {UserId}", session.Id, userId);
        return session.ToSummary();
    }
}
