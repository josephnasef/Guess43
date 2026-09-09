using Guess43.Infrastructure.Security;
using Microsoft.Extensions.Options;

namespace Guess43.Api.Security;

/// <summary>Reads and writes the Secure/HttpOnly refresh-token cookie.</summary>
public sealed class RefreshCookieManager(IOptions<RefreshTokenOptions> options)
{
    private const string CookiePath = "/api/auth";
    private readonly RefreshTokenOptions _options = options.Value;

    public string CookieName => _options.CookieName;

    public void Write(HttpResponse response, string token, DateTime expiresAtUtc)
    {
        response.Cookies.Append(CookieName, token, BuildOptions(expiresAtUtc));
    }

    public void Clear(HttpResponse response)
    {
        response.Cookies.Delete(CookieName, BuildOptions(DateTime.UnixEpoch));
    }

    public string? Read(HttpRequest request) =>
        request.Cookies.TryGetValue(CookieName, out var value) ? value : null;

    private static CookieOptions BuildOptions(DateTime expiresAtUtc) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Path = CookiePath,
        Expires = expiresAtUtc,
        IsEssential = true,
    };
}
