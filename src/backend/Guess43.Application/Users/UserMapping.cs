using Guess43.Application.Users.Contracts;
using Guess43.Domain.Users;

namespace Guess43.Application.Users;

internal static class UserMapping
{
    public static UserProfileResponse ToProfile(this User user) =>
        new(user.Id, user.Email, user.DisplayName, user.BestGuessCount, user.CreatedAtUtc);
}
