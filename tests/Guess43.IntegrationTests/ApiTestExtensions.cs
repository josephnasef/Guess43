using System.Net.Http.Json;
using System.Text.Json;

namespace Guess43.IntegrationTests;

internal static class ApiTestExtensions
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static async Task<T> ReadAsync<T>(this HttpResponseMessage response)
    {
        var value = await response.Content.ReadFromJsonAsync<T>(Json);
        return value ?? throw new InvalidOperationException("Response body was null.");
    }

    public static void UseBearer(this HttpClient client, string accessToken) =>
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
}

internal sealed record ProfileDto(Guid Id, string Email, string DisplayName, int? BestGuessCount, DateTime CreatedAtUtc);

internal sealed record AuthDto(string AccessToken, DateTime AccessTokenExpiresAtUtc, ProfileDto User);

internal sealed record GameSummaryDto(Guid Id, string Status, int GuessCount, DateTime StartedAtUtc, DateTime? CompletedAtUtc);

internal sealed record GuessDto(Guid GameId, string Outcome, int GuessCount, bool IsCompleted, int? PersonalBest);

internal sealed record RegisterBody(string Email, string DisplayName, string Password);

internal sealed record LoginBody(string Email, string Password);

internal sealed record GuessBody(int Guess);
