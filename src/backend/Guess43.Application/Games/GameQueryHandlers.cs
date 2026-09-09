using Guess43.Application.Abstractions;
using Guess43.Application.Common;
using Guess43.Application.Games.Contracts;
using Guess43.Domain.Games;
using Microsoft.Extensions.Logging;

namespace Guess43.Application.Games;

/// <summary>Restores the caller's active game, or null when none is in progress.</summary>
public sealed class GetActiveGameHandler(IGameSessionRepository games)
{
    public async Task<Result<GameSummaryResponse?>> HandleAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var active = await games.GetActiveForUserAsync(userId, cancellationToken);
        return active?.ToSummary();
    }
}

/// <summary>Returns a page of the caller's game history.</summary>
public sealed class GetGameHistoryHandler(IGameSessionRepository games)
{
    public const int MaxPageSize = 50;
    public const int DefaultPageSize = 10;

    public async Task<Result<PagedResult<GameSummaryResponse>>> HandleAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var safePage = page < 1 ? 1 : page;
        var safeSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

        var result = await games.GetHistoryAsync(userId, safePage, safeSize, cancellationToken);
        var items = result.Items.Select(g => g.ToSummary()).ToList();

        return new PagedResult<GameSummaryResponse>(items, result.Page, result.PageSize, result.TotalCount);
    }
}

/// <summary>Returns an owned game summary, or 404 when absent or owned by another user.</summary>
public sealed class GetGameByIdHandler(IGameSessionRepository games)
{
    public async Task<Result<GameSummaryResponse>> HandleAsync(
        Guid userId,
        Guid gameId,
        CancellationToken cancellationToken = default)
    {
        var session = await games.GetOwnedAsync(gameId, userId, cancellationToken);
        return session is null ? ApplicationErrors.GameNotFound : session.ToSummary();
    }
}

/// <summary>Deletes an owned, completed game without affecting the persisted personal best.</summary>
public sealed class DeleteGameHandler(
    IGameSessionRepository games,
    IUnitOfWork unitOfWork,
    ILogger<DeleteGameHandler> logger)
{
    public async Task<Result> HandleAsync(Guid userId, Guid gameId, CancellationToken cancellationToken = default)
    {
        var session = await games.GetOwnedAsync(gameId, userId, cancellationToken);
        if (session is null)
        {
            return ApplicationErrors.GameNotFound;
        }

        if (session.Status != GameStatus.Completed)
        {
            return ApplicationErrors.GameNotCompleted;
        }

        games.Remove(session);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Game {GameId} deleted for user {UserId}", gameId, userId);
        return Result.Success();
    }
}
