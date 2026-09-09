using Guess43.Application.Abstractions;
using Guess43.Application.Common;
using Guess43.Application.Statistics.Contracts;
using Guess43.Domain.Games;

namespace Guess43.Application.Statistics;

/// <summary>
/// Derives performance statistics and achievements from the caller's completed games.
/// Computed on demand from owned records; no separate achievements table is persisted.
/// </summary>
public sealed class GetPerformanceHandler(IUserRepository users, IGameSessionRepository games)
{
    public const int SharpShooterThreshold = 6;
    private const int RecentCount = 5;

    public async Task<Result<PerformanceResponse>> HandleAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return ApplicationErrors.UserNotFound;
        }

        var completed = await games.GetCompletedForUserAsync(userId, cancellationToken);

        var completedCount = completed.Count;
        int? bestScore = user.BestGuessCount;
        double? average = completedCount == 0 ? null : Math.Round(completed.Average(g => g.GuessCount), 2);

        var recent = completed
            .Where(g => g.CompletedAtUtc is not null)
            .OrderByDescending(g => g.CompletedAtUtc)
            .Take(RecentCount)
            .Select(g => new RecentGameResponse(g.Id, g.GuessCount, g.CompletedAtUtc!.Value))
            .ToList();

        var achievements = BuildAchievements(completed, bestScore);

        return new PerformanceResponse(completedCount, bestScore, average, recent, achievements);
    }

    private static List<AchievementResponse> BuildAchievements(
        IReadOnlyList<GameSession> completed,
        int? bestScore)
    {
        var hasFirstWin = completed.Count > 0;
        var hasSharpShooter = completed.Any(g => g.GuessCount <= SharpShooterThreshold);

        return
        [
            new AchievementResponse("FIRST_WIN", "First Win", "Win your first game.", hasFirstWin),
            new AchievementResponse(
                "SHARP_SHOOTER",
                "Sharp Shooter",
                $"Win a game in {SharpShooterThreshold} guesses or fewer.",
                hasSharpShooter),
            new AchievementResponse("PERSONAL_BEST", "Personal Best", "Set a personal best score.", bestScore is not null),
        ];
    }
}
