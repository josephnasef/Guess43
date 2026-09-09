using Guess43.Application.Common;
using Guess43.Domain.Games;

namespace Guess43.Application.Abstractions;

/// <summary>Persistence operations for the <see cref="GameSession"/> aggregate.</summary>
public interface IGameSessionRepository
{
    Task<GameSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Returns the owned game or null when it is absent or owned by another user.</summary>
    Task<GameSession?> GetOwnedAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    Task<GameSession?> GetActiveForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<PagedResult<GameSession>> GetHistoryAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>Returns all completed sessions for a user (used to derive statistics).</summary>
    Task<IReadOnlyList<GameSession>> GetCompletedForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    void Add(GameSession session);

    void Remove(GameSession session);

    /// <summary>Removes every game session owned by a user (account deletion).</summary>
    Task RemoveOwnedForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
