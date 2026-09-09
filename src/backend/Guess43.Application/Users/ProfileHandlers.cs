using Guess43.Application.Abstractions;
using Guess43.Application.Common;
using Guess43.Application.Users.Contracts;

namespace Guess43.Application.Users;

/// <summary>Returns the current user's profile including the nullable personal best.</summary>
public sealed class GetProfileHandler(IUserRepository users)
{
    public async Task<Result<UserProfileResponse>> HandleAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken);
        return user is null ? ApplicationErrors.UserNotFound : user.ToProfile();
    }
}

/// <summary>Updates the current user's display name.</summary>
public sealed class UpdateProfileHandler(IUserRepository users, IUnitOfWork unitOfWork, IClock clock)
{
    public async Task<Result<UserProfileResponse>> HandleAsync(
        Guid userId,
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return ApplicationErrors.UserNotFound;
        }

        user.UpdateProfile(request.DisplayName, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return user.ToProfile();
    }
}
