using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace Guess43.IntegrationTests;

[Collection(nameof(ApiCollection))]
public class AuthFlowTests(Guess43WebFactory factory)
{
    [Fact]
    public async Task Register_then_login_refresh_logout_flow()
    {
        var client = factory.CreateApiClient();
        var email = $"user_{Guid.NewGuid():N}@example.com";

        var register = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterBody(email, "Player One", "Password1"));
        register.StatusCode.Should().Be(HttpStatusCode.Created);
        var auth = await register.ReadAsync<AuthDto>();
        auth.AccessToken.Should().NotBeNullOrEmpty();
        auth.User.BestGuessCount.Should().BeNull();

        // Refresh rotates the token using the HttpOnly cookie.
        var refresh = await client.PostAsync("/api/auth/refresh", content: null);
        refresh.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshed = await refresh.ReadAsync<AuthDto>();
        refreshed.AccessToken.Should().NotBeNullOrEmpty();

        // Logout revokes the family.
        client.UseBearer(refreshed.AccessToken);
        var logout = await client.PostAsync("/api/auth/logout", content: null);
        logout.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // A refresh after logout is rejected.
        var afterLogout = await client.PostAsync("/api/auth/refresh", content: null);
        afterLogout.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_duplicate_email_returns_conflict_with_error_code()
    {
        var client = factory.CreateApiClient();
        var email = $"dupe_{Guid.NewGuid():N}@example.com";
        await client.PostAsJsonAsync("/api/auth/register", new RegisterBody(email, "Player", "Password1"));

        var second = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterBody(email.ToUpperInvariant(), "Player", "Password1"));

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body = await second.Content.ReadAsStringAsync();
        body.Should().Contain("EMAIL_ALREADY_EXISTS");
        body.Should().Contain("traceId");
    }

    [Fact]
    public async Task Login_with_wrong_password_returns_generic_invalid_credentials()
    {
        var client = factory.CreateApiClient();
        var email = $"login_{Guid.NewGuid():N}@example.com";
        await client.PostAsJsonAsync("/api/auth/register", new RegisterBody(email, "Player", "Password1"));

        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginBody(email, "WrongPass1"));

        login.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await login.Content.ReadAsStringAsync()).Should().Contain("INVALID_CREDENTIALS");
    }

    [Fact]
    public async Task Protected_endpoint_without_token_is_unauthorized()
    {
        var client = factory.CreateApiClient();

        var response = await client.GetAsync("/api/users/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
