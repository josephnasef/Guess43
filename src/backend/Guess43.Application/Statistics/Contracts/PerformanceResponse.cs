namespace Guess43.Application.Statistics.Contracts;

public sealed record AchievementResponse(string Code, string Name, string Description, bool Unlocked);

public sealed record PerformanceResponse(
    int CompletedGames,
    int? BestScore,
    double? AverageGuesses,
    IReadOnlyList<RecentGameResponse> RecentGames,
    IReadOnlyList<AchievementResponse> Achievements);

public sealed record RecentGameResponse(Guid Id, int GuessCount, DateTime CompletedAtUtc);
