namespace Guess43.Application.Users.Contracts;

/// <summary>Public profile view including the nullable personal best.</summary>
public sealed record UserProfileResponse(
    Guid Id,
    string Email,
    string DisplayName,
    int? BestGuessCount,
    DateTime CreatedAtUtc);
