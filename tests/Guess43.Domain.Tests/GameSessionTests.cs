using FluentAssertions;
using Guess43.Domain.Games;

namespace Guess43.Domain.Tests;

public class GameSessionTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private static GameSession StartWithTarget(int target)
        => GameSession.Start(Guid.NewGuid(), target, Now);

    [Fact]
    public void Start_creates_active_session_with_zero_guesses()
    {
        var session = StartWithTarget(20);

        session.Status.Should().Be(GameStatus.Active);
        session.GuessCount.Should().Be(0);
        session.CompletedAtUtc.Should().BeNull();
        session.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(GameLimits.MinNumber)]
    [InlineData(GameLimits.MaxNumber)]
    public void SubmitGuess_accepts_boundary_values(int guess)
    {
        var session = StartWithTarget(guess);

        var act = () => session.SubmitGuess(guess, Now);

        act.Should().NotThrow();
        session.GuessCount.Should().Be(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(GameLimits.MaxNumber + 1)]
    [InlineData(-5)]
    public void SubmitGuess_out_of_range_throws_and_does_not_increment(int guess)
    {
        var session = StartWithTarget(20);

        var act = () => session.SubmitGuess(guess, Now);

        act.Should().Throw<GuessOutOfRangeException>();
        session.GuessCount.Should().Be(0);
    }

    [Fact]
    public void SubmitGuess_returns_higher_when_target_is_greater()
    {
        var session = StartWithTarget(30);

        session.SubmitGuess(10, Now).Should().Be(GuessOutcome.Higher);
    }

    [Fact]
    public void SubmitGuess_returns_lower_when_target_is_smaller()
    {
        var session = StartWithTarget(10);

        session.SubmitGuess(30, Now).Should().Be(GuessOutcome.Lower);
    }

    [Fact]
    public void SubmitGuess_returns_correct_and_completes_on_match()
    {
        var session = StartWithTarget(25);

        var outcome = session.SubmitGuess(25, Now);

        outcome.Should().Be(GuessOutcome.Correct);
        session.Status.Should().Be(GameStatus.Completed);
        session.CompletedAtUtc.Should().Be(Now);
    }

    [Fact]
    public void Each_accepted_guess_increments_exactly_once()
    {
        var session = StartWithTarget(43);

        session.SubmitGuess(10, Now);
        session.SubmitGuess(20, Now);
        session.SubmitGuess(30, Now);

        session.GuessCount.Should().Be(3);
    }

    [Fact]
    public void SubmitGuess_after_completion_throws()
    {
        var session = StartWithTarget(5);
        session.SubmitGuess(5, Now);

        var act = () => session.SubmitGuess(4, Now);

        act.Should().Throw<GameAlreadyCompletedException>();
        session.GuessCount.Should().Be(1);
    }
}
