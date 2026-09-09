using Guess43.Api.Security;
using Guess43.Application.Statistics;
using Guess43.Application.Statistics.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Guess43.Api.Controllers;

[ApiController]
[Route("api/stats")]
[Authorize]
public sealed class StatsController : ApiControllerBase
{
    [HttpGet("performance")]
    [ProducesResponseType(typeof(PerformanceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Performance(
        [FromServices] GetPerformanceHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(User.GetUserId(), cancellationToken);
        return FromResult(result, Ok);
    }
}
