using Guess43.Api.Errors;
using Guess43.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace Guess43.Api.Controllers;

/// <summary>Base controller that turns Application results into HTTP responses.</summary>
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult FromResult<TValue>(Result<TValue> result, Func<TValue, IActionResult> onSuccess) =>
        result.IsSuccess ? onSuccess(result.Value) : Problem(result.Error);

    protected IActionResult FromResult(Result result, Func<IActionResult> onSuccess) =>
        result.IsSuccess ? onSuccess() : Problem(result.Error);

    protected IActionResult Ok<TValue>(Result<TValue> result) =>
        FromResult(result, value => Ok(value));

    protected IActionResult Problem(Error error)
    {
        var problem = ErrorResults.ToProblemDetails(error, HttpContext);
        return StatusCode(problem.Status ?? 400, problem);
    }
}
