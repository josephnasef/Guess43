using Guess43.Application.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Guess43.Api.Errors;

/// <summary>Maps Application <see cref="Error"/> values to RFC ProblemDetails responses.</summary>
public static class ErrorResults
{
    public static int ToStatusCode(ErrorCategory category) => category switch
    {
        ErrorCategory.Validation => StatusCodes.Status400BadRequest,
        ErrorCategory.NotFound => StatusCodes.Status404NotFound,
        ErrorCategory.Conflict => StatusCodes.Status409Conflict,
        ErrorCategory.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorCategory.Forbidden => StatusCodes.Status403Forbidden,
        _ => StatusCodes.Status400BadRequest,
    };

    public static ProblemDetails ToProblemDetails(Error error, HttpContext httpContext)
    {
        var status = ToStatusCode(error.Category);
        var problem = new ProblemDetails
        {
            Status = status,
            Title = TitleFor(error.Category),
            Detail = error.Description,
            Type = $"https://httpstatuses.io/{status}",
        };

        problem.Extensions["errorCode"] = error.Code;
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
        return problem;
    }

    private static string TitleFor(ErrorCategory category) => category switch
    {
        ErrorCategory.Validation => "Validation failed",
        ErrorCategory.NotFound => "Resource not found",
        ErrorCategory.Conflict => "Conflict",
        ErrorCategory.Unauthorized => "Unauthorized",
        ErrorCategory.Forbidden => "Forbidden",
        _ => "Request failed",
    };
}
