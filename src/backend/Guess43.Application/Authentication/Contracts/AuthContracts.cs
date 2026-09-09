using Guess43.Application.Users.Contracts;

namespace Guess43.Application.Authentication.Contracts;

public sealed record RegisterRequest(string Email, string DisplayName, string Password);

public sealed record LoginRequest(string Email, string Password);

/// <summary>
/// Internal result of an authentication use case. The API forwards the access token
/// to the SPA and writes the refresh token into a Secure/HttpOnly cookie.
/// </summary>
public sealed record AuthenticationResult(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc,
    UserProfileResponse User);

/// <summary>Response body returned to the SPA (never contains the refresh token).</summary>
public sealed record AuthenticationResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    UserProfileResponse User);
