namespace Guess43.Application.Common;

/// <summary>Classifies an expected failure so the API can map it to an HTTP status.</summary>
public enum ErrorCategory
{
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Unauthorized = 4,
    Forbidden = 5,
}

/// <summary>
/// A stable, serialisable description of an expected failure. Expected failures
/// are represented as values, never thrown as exceptions.
/// </summary>
public sealed record Error(string Code, string Description, ErrorCategory Category)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorCategory.Validation);
}
