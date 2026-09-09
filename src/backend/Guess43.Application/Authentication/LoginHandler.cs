using Guess43.Application.Abstractions;
using Guess43.Application.Authentication.Contracts;
using Guess43.Application.Common;
using Microsoft.Extensions.Logging;

namespace Guess43.Application.Authentication;

/// <summary>Authenticates a user with email and password.</summary>
public sealed class LoginHandler(
    IUserRepository users,
    IRefreshTokenRepository refreshTokens,
    IAccessTokenService accessTokens,
    IRefreshTokenService refreshTokenService,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    IClock clock,
    ILogger<LoginHandler> logger)
{
    public async Task<Result<AuthenticationResult>> HandleAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = Domain.Users.User.NormalizeEmail(request.Email);
        var user = await users.GetByNormalizedEmailAsync(normalizedEmail, cancellationToken);

        // Verify even when the user is missing to keep timing indistinguishable and
        // return the same generic error for unknown email vs wrong password.
        var hashToCheck = user?.PasswordHash ?? DummyHash;
        var verification = passwordHasher.Verify(hashToCheck, request.Password);

        if (user is null || verification == PasswordVerificationOutcome.Failed)
        {
            logger.LogInformation("Failed login attempt for normalized email {NormalizedEmail}", normalizedEmail);
            return ApplicationErrors.InvalidCredentials;
        }

        if (verification == PasswordVerificationOutcome.SuccessRehashNeeded)
        {
            user.SetPasswordHash(passwordHasher.Hash(request.Password), clock.UtcNow);
        }

        var result = IssueTokens.For(user, refreshTokens, accessTokens, refreshTokenService, clock);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("User {UserId} logged in", user.Id);

        return result;
    }

    // A precomputed hash used to equalise verification time when the account is missing.
    private const string DummyHash =
        "AQAAAAIAAYagAAAAEExampleDummyHashToEqualiseTimingXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX==";
}
