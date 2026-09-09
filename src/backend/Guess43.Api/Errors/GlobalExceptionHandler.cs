using Guess43.Api.Errors;
using Guess43.Application.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Guess43.Api.Errors;

/// <summary>
/// Central handler for unexpected exceptions. Expected business outcomes use Result;
/// this handler logs unexpected failures once and returns a sanitized response.
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problem = exception switch
        {
            ConcurrencyConflictException => ErrorResults.ToProblemDetails(
                ApplicationErrors.ConcurrencyConflict, httpContext),
            UniqueConstraintConflictException => ErrorResults.ToProblemDetails(
                new Error("CONFLICT", "The operation conflicts with existing data.", ErrorCategory.Conflict),
                httpContext),
            _ => UnexpectedProblem(httpContext, exception),
        };

        httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }

    private ProblemDetails UnexpectedProblem(HttpContext httpContext, Exception exception)
    {
        // Log the full internal exception exactly once; never expose details to the client.
        logger.LogError(exception, "Unhandled exception for {Method} {Path}",
            httpContext.Request.Method, httpContext.Request.Path);

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred",
            Detail = "An unexpected error occurred while processing the request.",
            Type = "https://httpstatuses.io/500",
        };
        problem.Extensions["errorCode"] = "INTERNAL_ERROR";
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
        return problem;
    }
}
