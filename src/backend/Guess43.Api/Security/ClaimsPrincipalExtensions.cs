using System.Security.Claims;

namespace Guess43.Api.Security;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Reads the authenticated user's id from the validated 'sub' claim.</summary>
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue("sub") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id)
            ? id
            : throw new InvalidOperationException("The authenticated principal has no valid user id claim.");
    }
}
