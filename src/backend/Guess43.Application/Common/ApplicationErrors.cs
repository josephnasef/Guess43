namespace Guess43.Application.Common;

/// <summary>Catalogue of stable error codes shared with the API and frontend.</summary>
public static class ApplicationErrors
{
    public static Error Validation(string description) =>
        new("VALIDATION_ERROR", description, ErrorCategory.Validation);

    public static readonly Error EmailAlreadyExists =
        new("EMAIL_ALREADY_EXISTS", "An account with this email already exists.", ErrorCategory.Conflict);

    public static readonly Error InvalidCredentials =
        new("INVALID_CREDENTIALS", "The email or password is incorrect.", ErrorCategory.Unauthorized);

    public static readonly Error InvalidRefreshToken =
        new("INVALID_REFRESH_TOKEN", "The refresh token is missing, expired, or revoked.", ErrorCategory.Unauthorized);

    public static readonly Error UserNotFound =
        new("USER_NOT_FOUND", "The user could not be found.", ErrorCategory.NotFound);

    public static readonly Error GameNotFound =
        new("GAME_NOT_FOUND", "The requested game could not be found.", ErrorCategory.NotFound);

    public static readonly Error GameAlreadyCompleted =
        new("GAME_ALREADY_COMPLETED", "This game is already completed.", ErrorCategory.Conflict);

    public static readonly Error GuessOutOfRange =
        new("GUESS_OUT_OF_RANGE", "The guess must be between 1 and 43.", ErrorCategory.Validation);

    public static readonly Error ConcurrencyConflict =
        new("CONCURRENCY_CONFLICT", "The resource was modified concurrently. Please retry.", ErrorCategory.Conflict);

    public static readonly Error GameNotCompleted =
        new("GAME_NOT_COMPLETED", "Only completed games can be deleted.", ErrorCategory.Conflict);
}
