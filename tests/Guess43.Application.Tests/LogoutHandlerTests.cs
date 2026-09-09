using FluentAssertions;
using Guess43.Application.Abstractions;
using Guess43.Application.Authentication;
using Guess43.Domain.Users;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Guess43.Application.Tests;

public class LogoutHandlerTests
{
    private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();
    private readonly IRefreshTokenService _refreshTokenService = Substitute.For<IRefreshTokenService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly TestClock _clock = TestClock.Default;

    private LogoutHandler CreateSut() => new(
        _refreshTokens, _refreshTokenService, _unitOfWork, _clock, NullLogger<LogoutHandler>.Instance);

    [Fact]
    public async Task Logout_revokes_token_family_for_presented_token()
    {
        var userId = Guid.NewGuid();
        _refreshTokenService.Hash("raw").Returns("hash");
        _refreshTokens.GetByHashAsync("hash", Arg.Any<CancellationToken>())
            .Returns(RefreshToken.Issue(userId, "hash", _clock.UtcNow, _clock.UtcNow.AddDays(7)));
        var sut = CreateSut();

        var result = await sut.HandleAsync("raw");

        result.IsSuccess.Should().BeTrue();
        await _refreshTokens.Received(1).RevokeAllForUserAsync(userId, _clock.UtcNow, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Logout_is_idempotent_when_no_token_present()
    {
        var sut = CreateSut();

        var result = await sut.HandleAsync(null);

        result.IsSuccess.Should().BeTrue();
        await _refreshTokens.DidNotReceive()
            .RevokeAllForUserAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }
}
