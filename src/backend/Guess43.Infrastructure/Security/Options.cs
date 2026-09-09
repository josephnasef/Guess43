namespace Guess43.Infrastructure.Security;

/// <summary>Configuration for issuing and validating JWT access tokens.</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "guess43";

    public string Audience { get; set; } = "guess43-client";

    /// <summary>Symmetric signing key. Supplied via configuration/secret, never hard-coded.</summary>
    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 15;
}

/// <summary>Configuration for rotating refresh tokens.</summary>
public sealed class RefreshTokenOptions
{
    public const string SectionName = "RefreshToken";

    public int Days { get; set; } = 7;

    /// <summary>Name of the Secure/HttpOnly cookie carrying the refresh token.</summary>
    public string CookieName { get; set; } = "guess43_rt";
}
