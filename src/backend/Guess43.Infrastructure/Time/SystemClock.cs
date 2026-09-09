using System.Security.Cryptography;
using Guess43.Application.Abstractions;
using Guess43.Domain.Games;

namespace Guess43.Infrastructure.Time;

/// <summary>Production clock returning the current UTC time.</summary>
public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}

/// <summary>Generates the secret number using a cryptographically secure RNG.</summary>
public sealed class CryptoSecretNumberGenerator : ISecretNumberGenerator
{
    public int Next() =>
        RandomNumberGenerator.GetInt32(GameLimits.MinNumber, GameLimits.RandomExclusiveUpperBound);
}
