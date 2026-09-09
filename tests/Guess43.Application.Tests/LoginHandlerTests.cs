using FluentAssertions;
using Guess43.Application.Abstractions;
using Guess43.Application.Authentication;
using Guess43.Application.Authentication.Contracts;
using Guess43.Domain.Users;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Guess43.Application.Tests;

public class LoginHandlerTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();
    private readonly IAccessTokenService _accessTokens = Substitute.For<IAccessTokenService>();
    private readonly IRefreshTokenService _refreshTokenService = Substitute.For<IRefreshTokenService>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly TestClock _clock = TestClock.Default;

    private LoginHandler CreateSut()
    {
        _accessTokens.Issue(Arg.Any<User>()).Returns(new AccessToken("access", _clock.UtcNow.AddMinutes(15)));
        _refreshTokenService.Issue().Returns(new IssuedRefreshToken("raw", "hash", _clock.UtcNow.AddDays(7)));
        return new LoginHandler(
            _users, _refreshTokens, _accessTokens, _refreshTokenService, _passwordHasher, _unitOfWork, _clock,
            NullLogger<LoginHandler>.Instance);
    }

    private static User ExistingUser() => User.Register("user@example.com", "User", "stored-hash", TestClock.Default.UtcNow);

    [Fact]
    public async Task Login_with_unknown_email_returns_invalid_credentials()
    {
        _users.GetByNormalizedEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((User?)null);
        _passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(PasswordVerificationOutcome.Failed);
        var sut = CreateSut();

        var result = await sut.HandleAsync(new LoginRequest("missing@example.com", "Password1"));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("INVALID_CREDENTIALS");
    }

    [Fact]
    public async Task Login_with_wrong_password_returns_same_invalid_credentials()
    {
        _users.GetByNormalizedEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(ExistingUser());
        _passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(PasswordVerificationOutcome.Failed);
        var sut = CreateSut();

        var result = await sut.HandleAsync(new LoginRequest("user@example.com", "WrongPass1"));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("INVALID_CREDENTIALS");
    }

    [Fact]
    public async Task Login_verifies_password_even_when_user_missing_to_equalise_timing()
    {
        _users.GetByNormalizedEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((User?)null);
        _passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(PasswordVerificationOutcome.Failed);
        var sut = CreateSut();

        await sut.HandleAsync(new LoginRequest("missing@example.com", "Password1"));

        _passwordHasher.Received(1).Verify(Arg.Any<string>(), "Password1");
    }

    [Fact]
    public async Task Login_success_issues_tokens_and_saves()
    {
        _users.GetByNormalizedEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(ExistingUser());
        _passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(PasswordVerificationOutcome.Success);
        var sut = CreateSut();

        var result = await sut.HandleAsync(new LoginRequest("user@example.com", "Password1"));

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
