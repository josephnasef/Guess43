using Guess43.Application.Abstractions;
using Guess43.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Guess43.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
        dbContext.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);

    public Task<bool> ExistsByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
        dbContext.Users.AnyAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);

    public void Add(User user) => dbContext.Users.Add(user);

    public void Remove(User user) => dbContext.Users.Remove(user);
}
