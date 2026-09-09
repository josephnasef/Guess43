using Guess43.Domain.Users;

namespace Guess43.Application.Abstractions;

/// <summary>Persistence operations for the <see cref="User"/> aggregate.</summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<User?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    void Add(User user);

    void Remove(User user);
}
