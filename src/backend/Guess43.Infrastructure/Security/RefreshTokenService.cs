using System.Security.Cryptography;
using System.Text;
using Guess43.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace Guess43.Infrastructure.Security;

/// <summary>
/// Creates opaque refresh tokens and hashes them with SHA-256. Only the hash is
/// persisted; the raw value is returned to the caller exactly once.
/// </summary>
public sealed class RefreshTokenService(IOptions<RefreshTokenOptions> options, IClock clock) : IRefreshTokenService
{
    private const int TokenByteLength = 32;
    private readonly RefreshTokenOptions _options = options.Value;

    public IssuedRefreshToken Issue()
    {
        var bytes = RandomNumberGenerator.GetBytes(TokenByteLength);
        var rawValue = Base64UrlEncode(bytes);
        var expiresAt = clock.UtcNow.AddDays(_options.Days);
        return new IssuedRefreshToken(rawValue, Hash(rawValue), expiresAt);
    }

    public string Hash(string rawValue)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawValue));
        return Convert.ToHexString(hashBytes);
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
