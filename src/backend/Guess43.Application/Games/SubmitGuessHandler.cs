using Guess43.Application.Abstractions;
using Guess43.Application.Common;
using Guess43.Application.Games.Contracts;
using Guess43.Domain.Games;
using Microsoft.Extensions.Logging;

namespace Guess43.Application.Games;

/// <summary>
/// Submits a guess to the caller's active game. When the guess wins, completing the
/// game and improving the personal best are persisted in a single commit (atomic).
/// </summary>
public sealed class SubmitGuessHandler(
    IGameSessionRepository games,
    IUserRepository users,
    IUnitOfWork unitOfWork,
    IClock clock,
    ILogger<SubmitGuessHandler> logger)
{
    public async Task<Result<GuessResponse>> HandleAsync(
        Guid userId,
        Guid gameId,
        SubmitGuessRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!GameLimits.IsInRange(request.Guess))
        {
            return ApplicationErrors.GuessOutOfRange;
        }

        var session = await games.GetOwnedAsync(gameId, userId, cancellationToken);
        if (session is null)
        {
            return ApplicationErrors.GameNotFound;
        }

        if (session.Status == GameStatus.Completed)
        {
            return ApplicationErrors.GameAlreadyCompleted;
        }

        var now = clock.UtcNow;
        var outcome = session.SubmitGuess(request.Guess, now);

        int? personalBest = null;
        if (outcome == GuessOutcome.Correct)
        {
            var user = await users.GetByIdAsync(userId, cancellationToken);
            if (user is null)
            {
                return ApplicationErrors.UserNotFound;
            }

            var newRecord = user.TryImprovePersonalBest(session.GuessCount, now);
            personalBest = user.BestGuessCount;

            if (newRecord)
            {
                logger.LogInformation("User {UserId} set a new personal best of {Best}", userId, user.BestGuessCount);
            }
        }

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            return ApplicationErrors.ConcurrencyConflict;
        }

        return new GuessResponse(
            session.Id,
            outcome.ToString(),
            session.GuessCount,
            session.Status == GameStatus.Completed,
            personalBest);
    }
}
