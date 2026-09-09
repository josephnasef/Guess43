using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Guess43.Application.Abstractions;
using Guess43.Domain.Users;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Guess43.Infrastructure.Security;

/// <summary>Issues short-lived signed JWT access tokens.</summary>
public sealed class JwtAccessTokenService(IOptions<JwtOptions> options, IClock clock) : IAccessTokenService
{
    private readonly JwtOptions _options = options.Value;

    public AccessToken Issue(User user)
    {
        var expiresAt = clock.UtcNow.AddMinutes(_options.AccessTokenMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.DisplayName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: clock.UtcNow,
            expires: expiresAt,
            signingCredentials: credentials);

        var value = new JwtSecurityTokenHandler().WriteToken(token);
        return new AccessToken(value, expiresAt);
    }
}
