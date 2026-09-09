using Guess43.Application.Abstractions;
using Guess43.Application.Authentication.Contracts;
using Guess43.Application.Common;
using Guess43.Application.Users;
using Guess43.Domain.Users;
using Microsoft.Extensions.Logging;

namespace Guess43.Application.Authentication;

/// <summary>Registers a new account and issues the first token pair.</summary>
public sealed class RegisterUserHandler(
    IUserRepository users,
    IRefreshTokenRepository refreshTokens,
    IAccessTokenService accessTokens,
    IRefreshTokenService refreshTokenService,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    IClock clock,
    ILogger<RegisterUserHandler> logger)
{
    public async Task<Result<AuthenticationResult>> HandleAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = User.NormalizeEmail(request.Email);
        if (await users.ExistsByNormalizedEmailAsync(normalizedEmail, cancellationToken))
        {
            return ApplicationErrors.EmailAlreadyExists;
        }

        var passwordHash = passwordHasher.Hash(request.Password);
        var user = User.Register(request.Email, request.DisplayName, passwordHash, clock.UtcNow);
        users.Add(user);

        var result = IssueTokens.For(user, refreshTokens, accessTokens, refreshTokenService, clock);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Account created for user {UserId}", user.Id);

        return result;
    }
}
