using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace Guess43.IntegrationTests;

[Collection(nameof(ApiCollection))]
public class GameFlowTests(Guess43WebFactory factory)
{
    private static async Task<AuthDto> RegisterAsync(HttpClient client)
    {
        var email = $"game_{Guid.NewGuid():N}@example.com";
        var response = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterBody(email, "Gamer", "Password1"));
        var auth = await response.ReadAsync<AuthDto>();
        client.UseBearer(auth.AccessToken);
        return auth;
    }

    /// <summary>Wins by binary search over 1-43 using higher/lower feedback.</summary>
    private static async Task<GuessDto> PlayToWinAsync(HttpClient client, Guid gameId)
    {
        int lo = 1, hi = 43;
        GuessDto? last = null;
        while (lo <= hi)
        {
            var guess = (lo + hi) / 2;
            var response = await client.PostAsJsonAsync($"/api/games/{gameId}/guesses", new GuessBody(guess));
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            last = await response.ReadAsync<GuessDto>();
            if (last.IsCompleted)
            {
                return last;
            }

            if (last.Outcome == "Higher")
            {
                lo = guess + 1;
            }
            else
            {
                hi = guess - 1;
            }
        }

        throw new InvalidOperationException("Failed to win the game.");
    }

    [Fact]
    public async Task Play_to_win_persists_personal_best_and_shows_on_relogin()
    {
        var client = factory.CreateApiClient();
        var auth = await RegisterAsync(client);

        var start = await client.PostAsync("/api/games", content: null);
        start.StatusCode.Should().Be(HttpStatusCode.OK);
        var game = await start.ReadAsync<GameSummaryDto>();

        var win = await PlayToWinAsync(client, game.Id);
        win.IsCompleted.Should().BeTrue();
        win.PersonalBest.Should().Be(win.GuessCount);

        var profile = await (await client.GetAsync("/api/users/me")).ReadAsync<ProfileDto>();
        profile.BestGuessCount.Should().Be(win.GuessCount);

        // Re-login on a fresh client and confirm the personal best is restored.
        var fresh = factory.CreateApiClient();
        var login = await fresh.PostAsJsonAsync("/api/auth/login", new LoginBody(auth.User.Email, "Password1"));
        var reAuth = await login.ReadAsync<AuthDto>();
        reAuth.User.BestGuessCount.Should().Be(win.GuessCount);
    }

    [Fact]
    public async Task Start_game_is_idempotent_while_active()
    {
        var client = factory.CreateApiClient();
        await RegisterAsync(client);

        var first = await (await client.PostAsync("/api/games", content: null)).ReadAsync<GameSummaryDto>();
        var second = await (await client.PostAsync("/api/games", content: null)).ReadAsync<GameSummaryDto>();

        second.Id.Should().Be(first.Id);
    }

    [Fact]
    public async Task Target_number_is_never_serialized()
    {
        var client = factory.CreateApiClient();
        await RegisterAsync(client);
        var game = await (await client.PostAsync("/api/games", content: null)).ReadAsync<GameSummaryDto>();
        await client.PostAsJsonAsync($"/api/games/{game.Id}/guesses", new GuessBody(20));

        var detail = await (await client.GetAsync($"/api/games/{game.Id}")).Content.ReadAsStringAsync();
        var history = await (await client.GetAsync("/api/games?page=1&pageSize=10")).Content.ReadAsStringAsync();

        detail.ToLowerInvariant().Should().NotContain("target");
        history.ToLowerInvariant().Should().NotContain("target");
    }

    [Fact]
    public async Task Cross_user_access_returns_not_found_without_leaking()
    {
        var owner = factory.CreateApiClient();
        await RegisterAsync(owner);
        var game = await (await owner.PostAsync("/api/games", content: null)).ReadAsync<GameSummaryDto>();

        var attacker = factory.CreateApiClient();
        await RegisterAsync(attacker);
        var response = await attacker.GetAsync($"/api/games/{game.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await response.Content.ReadAsStringAsync()).Should().Contain("GAME_NOT_FOUND");
    }

    [Fact]
    public async Task Guess_out_of_range_returns_validation_error()
    {
        var client = factory.CreateApiClient();
        await RegisterAsync(client);
        var game = await (await client.PostAsync("/api/games", content: null)).ReadAsync<GameSummaryDto>();

        var response = await client.PostAsJsonAsync($"/api/games/{game.Id}/guesses", new GuessBody(99));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("VALIDATION_ERROR");
    }
}
