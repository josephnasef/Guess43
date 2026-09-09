using FluentAssertions;
using Guess43.Domain.Users;

namespace Guess43.Domain.Tests;

public class UserTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private static User NewUser()
        => User.Register("Player@Example.com", "Player One", "hash", Now);

    [Fact]
    public void Register_normalizes_email_and_starts_with_no_personal_best()
    {
        var user = NewUser();

        user.NormalizedEmail.Should().Be("PLAYER@EXAMPLE.COM");
        user.Email.Should().Be("Player@Example.com");
        user.BestGuessCount.Should().BeNull();
    }

    [Fact]
    public void TryImprovePersonalBest_sets_initial_best()
    {
        var user = NewUser();

        var improved = user.TryImprovePersonalBest(7, Now);

        improved.Should().BeTrue();
        user.BestGuessCount.Should().Be(7);
    }

    [Fact]
    public void TryImprovePersonalBest_improves_when_lower()
    {
        var user = NewUser();
        user.TryImprovePersonalBest(7, Now);

        var improved = user.TryImprovePersonalBest(4, Now);

        improved.Should().BeTrue();
        user.BestGuessCount.Should().Be(4);
    }

    [Fact]
    public void TryImprovePersonalBest_never_worsens()
    {
        var user = NewUser();
        user.TryImprovePersonalBest(4, Now);

        var improved = user.TryImprovePersonalBest(9, Now);

        improved.Should().BeFalse();
        user.BestGuessCount.Should().Be(4);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void TryImprovePersonalBest_rejects_non_positive(int guessCount)
    {
        var user = NewUser();

        var act = () => user.TryImprovePersonalBest(guessCount, Now);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
