using Guess43.Application.Abstractions;
using Guess43.Application.Common;
using Guess43.Domain.Games;
using Microsoft.EntityFrameworkCore;

namespace Guess43.Infrastructure.Persistence.Repositories;

internal sealed class GameSessionRepository(AppDbContext dbContext) : IGameSessionRepository
{
    public Task<GameSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.GameSessions.FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

    public Task<GameSession?> GetOwnedAsync(Guid id, Guid userId, CancellationToken cancellationToken = default) =>
        dbContext.GameSessions.FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId, cancellationToken);

    public Task<GameSession?> GetActiveForUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        dbContext.GameSessions.FirstOrDefaultAsync(
            g => g.UserId == userId && g.Status == GameStatus.Active,
            cancellationToken);

    public async Task<PagedResult<GameSession>> GetHistoryAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.GameSessions
            .AsNoTracking()
            .Where(g => g.UserId == userId)
            .OrderByDescending(g => g.StartedAtUtc);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<GameSession>(items, page, pageSize, totalCount);
    }

    public async Task<IReadOnlyList<GameSession>> GetCompletedForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await dbContext.GameSessions
            .AsNoTracking()
            .Where(g => g.UserId == userId && g.Status == GameStatus.Completed)
            .ToListAsync(cancellationToken);

    public void Add(GameSession session) => dbContext.GameSessions.Add(session);

    public void Remove(GameSession session) => dbContext.GameSessions.Remove(session);

    public async Task RemoveOwnedForUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await dbContext.GameSessions
            .Where(g => g.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
}
