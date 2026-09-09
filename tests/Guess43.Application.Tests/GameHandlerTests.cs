using FluentAssertions;
using Guess43.Application.Abstractions;
using Guess43.Application.Games;
using Guess43.Application.Games.Contracts;
using Guess43.Domain.Games;
using Guess43.Domain.Users;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Guess43.Application.Tests;

public class GameHandlerTests
{
    private readonly IGameSessionRepository _games = Substitute.For<IGameSessionRepository>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly TestClock _clock = TestClock.Default;
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public async Task StartGame_returns_existing_active_session_without_creating_new()
    {
        var active = GameSession.Start(_userId, 20, _clock.UtcNow);
        _games.GetActiveForUserAsync(_userId, Arg.Any<CancellationToken>()).Returns(active);
        var sut = new StartGameHandler(_games, new StubSecretNumberGenerator(7), _unitOfWork, _clock,
            NullLogger<StartGameHandler>.Instance);

        var result = await sut.HandleAsync(_userId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(active.Id);
        _games.DidNotReceive().Add(Arg.Any<GameSession>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartGame_creates_new_session_when_none_active()
    {
        _games.GetActiveForUserAsync(_userId, Arg.Any<CancellationToken>()).Returns((GameSession?)null);
        var sut = new StartGameHandler(_games, new StubSecretNumberGenerator(7), _unitOfWork, _clock,
            NullLogger<StartGameHandler>.Instance);

        var result = await sut.HandleAsync(_userId);

        result.IsSuccess.Should().BeTrue();
        _games.Received(1).Add(Arg.Any<GameSession>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SubmitGuess_winning_updates_personal_best_in_single_commit()
    {
        var session = GameSession.Start(_userId, 25, _clock.UtcNow);
        var user = User.Register("user@example.com", "User", "hash", _clock.UtcNow);
        _games.GetOwnedAsync(session.Id, _userId, Arg.Any<CancellationToken>()).Returns(session);
        _users.GetByIdAsync(_userId, Arg.Any<CancellationToken>()).Returns(user);
        var sut = new SubmitGuessHandler(_games, _users, _unitOfWork, _clock, NullLogger<SubmitGuessHandler>.Instance);

        var result = await sut.HandleAsync(_userId, session.Id, new SubmitGuessRequest(25));

        result.IsSuccess.Should().BeTrue();
        result.Value.Outcome.Should().Be("Correct");
        result.Value.IsCompleted.Should().BeTrue();
        result.Value.PersonalBest.Should().Be(1);
        user.BestGuessCount.Should().Be(1);
        // Completion and personal-best update persist together in exactly one commit.
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SubmitGuess_out_of_range_returns_error_without_loading_game()
    {
        var sut = new SubmitGuessHandler(_games, _users, _unitOfWork, _clock, NullLogger<SubmitGuessHandler>.Instance);

        var result = await sut.HandleAsync(_userId, Guid.NewGuid(), new SubmitGuessRequest(99));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("GUESS_OUT_OF_RANGE");
        await _games.DidNotReceive().GetOwnedAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SubmitGuess_on_missing_or_foreign_game_returns_not_found()
    {
        _games.GetOwnedAsync(Arg.Any<Guid>(), _userId, Arg.Any<CancellationToken>()).Returns((GameSession?)null);
        var sut = new SubmitGuessHandler(_games, _users, _unitOfWork, _clock, NullLogger<SubmitGuessHandler>.Instance);

        var result = await sut.HandleAsync(_userId, Guid.NewGuid(), new SubmitGuessRequest(10));

        result.Error.Code.Should().Be("GAME_NOT_FOUND");
    }

    [Fact]
    public async Task DeleteGame_rejects_active_game()
    {
        var session = GameSession.Start(_userId, 10, _clock.UtcNow);
        _games.GetOwnedAsync(session.Id, _userId, Arg.Any<CancellationToken>()).Returns(session);
        var sut = new DeleteGameHandler(_games, _unitOfWork, NullLogger<DeleteGameHandler>.Instance);

        var result = await sut.HandleAsync(_userId, session.Id);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("GAME_NOT_COMPLETED");
        _games.DidNotReceive().Remove(Arg.Any<GameSession>());
    }

    [Fact]
    public async Task DeleteGame_removes_completed_owned_game()
    {
        var session = GameSession.Start(_userId, 10, _clock.UtcNow);
        session.SubmitGuess(10, _clock.UtcNow);
        _games.GetOwnedAsync(session.Id, _userId, Arg.Any<CancellationToken>()).Returns(session);
        var sut = new DeleteGameHandler(_games, _unitOfWork, NullLogger<DeleteGameHandler>.Instance);

        var result = await sut.HandleAsync(_userId, session.Id);

        result.IsSuccess.Should().BeTrue();
        _games.Received(1).Remove(session);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
