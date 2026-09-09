using Guess43.Application.Abstractions;

namespace Guess43.Application.Tests;

internal sealed class TestClock(DateTime utcNow) : IClock
{
    public DateTime UtcNow { get; set; } = utcNow;

    public static TestClock Default => new(new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc));
}

internal sealed class StubSecretNumberGenerator(int value) : ISecretNumberGenerator
{
    public int Next() => value;
}
