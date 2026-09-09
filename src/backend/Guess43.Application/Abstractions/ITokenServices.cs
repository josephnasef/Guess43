using Guess43.Domain.Users;

namespace Guess43.Application.Abstractions;

/// <summary>An issued JWT access token and its absolute expiry.</summary>
public sealed record AccessToken(string Value, DateTime ExpiresAtUtc);

/// <summary>A freshly minted refresh token: the raw value (sent to the client once) and its stored hash.</summary>
public sealed record IssuedRefreshToken(string RawValue, string TokenHash, DateTime ExpiresAtUtc);

/// <summary>Issues short-lived JWT access tokens.</summary>
public interface IAccessTokenService
{
    AccessToken Issue(User user);
}

/// <summary>Creates and hashes rotating refresh tokens.</summary>
public interface IRefreshTokenService
{
    IssuedRefreshToken Issue();

    /// <summary>Hashes a raw refresh token for constant-time lookup and comparison.</summary>
    string Hash(string rawValue);
}
