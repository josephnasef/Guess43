using Guess43.Application.Abstractions;
using Guess43.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Guess43.Infrastructure.Persistence.Repositories;

internal sealed class RefreshTokenRepository(AppDbContext dbContext) : IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

    public void Add(RefreshToken token) => dbContext.RefreshTokens.Add(token);

    public async Task RevokeAllForUserAsync(Guid userId, DateTime nowUtc, CancellationToken cancellationToken = default)
    {
        var activeTokens = await dbContext.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.Revoke(nowUtc);
        }
    }

    public async Task RemoveAllForUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await dbContext.RefreshTokens
            .Where(t => t.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
}
