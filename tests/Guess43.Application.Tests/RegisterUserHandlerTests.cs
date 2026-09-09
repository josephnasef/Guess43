using FluentAssertions;
using Guess43.Application.Abstractions;
using Guess43.Application.Authentication;
using Guess43.Application.Authentication.Contracts;
using Guess43.Domain.Users;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Guess43.Application.Tests;

public class RegisterUserHandlerTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();
    private readonly IAccessTokenService _accessTokens = Substitute.For<IAccessTokenService>();
    private readonly IRefreshTokenService _refreshTokenService = Substitute.For<IRefreshTokenService>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly TestClock _clock = TestClock.Default;

    private RegisterUserHandler CreateSut()
    {
        _accessTokens.Issue(Arg.Any<User>()).Returns(new AccessToken("access", _clock.UtcNow.AddMinutes(15)));
        _refreshTokenService.Issue().Returns(new IssuedRefreshToken("raw", "hash", _clock.UtcNow.AddDays(7)));
        _passwordHasher.Hash(Arg.Any<string>()).Returns("hashed");
        return new RegisterUserHandler(
            _users, _refreshTokens, _accessTokens, _refreshTokenService, _passwordHasher, _unitOfWork, _clock,
            NullLogger<RegisterUserHandler>.Instance);
    }

    [Fact]
    public async Task Register_rejects_duplicate_normalized_email()
    {
        _users.ExistsByNormalizedEmailAsync("USER@EXAMPLE.COM", Arg.Any<CancellationToken>()).Returns(true);
        var sut = CreateSut();

        var result = await sut.HandleAsync(new RegisterRequest("user@example.com", "User", "Password1"));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("EMAIL_ALREADY_EXISTS");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Register_saves_once_on_success_and_issues_tokens()
    {
        _users.ExistsByNormalizedEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        var sut = CreateSut();

        var result = await sut.HandleAsync(new RegisterRequest("New@Example.com", "New Player", "Password1"));

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access");
        result.Value.RefreshToken.Should().Be("raw");
        result.Value.User.Email.Should().Be("New@Example.com");
        _users.Received(1).Add(Arg.Any<User>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
