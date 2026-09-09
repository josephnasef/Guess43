using Guess43.Application.Abstractions;
using Guess43.Application.Authentication.Contracts;
using Guess43.Application.Users;
using Guess43.Domain.Users;

namespace Guess43.Application.Authentication;

/// <summary>Shared helper that issues an access token plus a stored refresh token.</summary>
internal static class IssueTokens
{
    public static AuthenticationResult For(
        User user,
        IRefreshTokenRepository refreshTokens,
        IAccessTokenService accessTokens,
        IRefreshTokenService refreshTokenService,
        IClock clock)
    {
        var access = accessTokens.Issue(user);
        var refresh = refreshTokenService.Issue();

        refreshTokens.Add(RefreshToken.Issue(user.Id, refresh.TokenHash, clock.UtcNow, refresh.ExpiresAtUtc));

        return new AuthenticationResult(
            access.Value,
            access.ExpiresAtUtc,
            refresh.RawValue,
            refresh.ExpiresAtUtc,
            user.ToProfile());
    }
}
