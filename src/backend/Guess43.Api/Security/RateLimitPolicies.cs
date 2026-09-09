namespace Guess43.Api.Security;

/// <summary>Named rate-limiting policies applied to sensitive endpoints.</summary>
public static class RateLimitPolicies
{
    public const string Authentication = "auth";
    public const string Guessing = "guess";
}
