using Guess43.Application.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace Guess43.Infrastructure.Security;

/// <summary>Adapts ASP.NET Core's <see cref="PasswordHasher{TUser}"/> to the Application port.</summary>
public sealed class IdentityPasswordHasher : IPasswordHasher
{
    private static readonly object Subject = new();
    private readonly PasswordHasher<object> _inner = new();

    public string Hash(string password) => _inner.HashPassword(Subject, password);

    public PasswordVerificationOutcome Verify(string passwordHash, string providedPassword)
    {
        try
        {
            return _inner.VerifyHashedPassword(Subject, passwordHash, providedPassword) switch
            {
                PasswordVerificationResult.Success => PasswordVerificationOutcome.Success,
                PasswordVerificationResult.SuccessRehashNeeded => PasswordVerificationOutcome.SuccessRehashNeeded,
                _ => PasswordVerificationOutcome.Failed,
            };
        }
        catch (FormatException)
        {
            // Malformed stored hash (e.g. the equalising dummy hash) never verifies.
            return PasswordVerificationOutcome.Failed;
        }
    }
}
