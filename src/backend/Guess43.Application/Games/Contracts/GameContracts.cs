using Guess43.Domain.Games;

namespace Guess43.Application.Games.Contracts;

/// <summary>Safe projection of a game session. Never includes the secret target number.</summary>
public sealed record GameSummaryResponse(
    Guid Id,
    string Status,
    int GuessCount,
    DateTime StartedAtUtc,
    DateTime? CompletedAtUtc);

public sealed record SubmitGuessRequest(int Guess);

/// <summary>Result of a guess: outcome, accepted count, completion state, and personal best.</summary>
public sealed record GuessResponse(
    Guid GameId,
    string Outcome,
    int GuessCount,
    bool IsCompleted,
    int? PersonalBest);

internal static class GameMapping
{
    public static GameSummaryResponse ToSummary(this GameSession session) =>
        new(session.Id, session.Status.ToString(), session.GuessCount, session.StartedAtUtc, session.CompletedAtUtc);
}
