using Guess43.Api.Security;
using Guess43.Application.Common;
using Guess43.Application.Games;
using Guess43.Application.Games.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Guess43.Api.Controllers;

[ApiController]
[Route("api/games")]
[Authorize]
public sealed class GamesController : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(GameSummaryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Start(
        [FromServices] StartGameHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(User.GetUserId(), cancellationToken);
        return FromResult(result, Ok);
    }

    [HttpGet("active")]
    [ProducesResponseType(typeof(GameSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Active(
        [FromServices] GetActiveGameHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(User.GetUserId(), cancellationToken);
        return FromResult(result, summary => summary is null ? NoContent() : Ok(summary));
    }

    [HttpPost("{gameId:guid}/guesses")]
    [EnableRateLimiting(RateLimitPolicies.Guessing)]
    [ProducesResponseType(typeof(GuessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Guess(
        [FromRoute] Guid gameId,
        [FromBody] SubmitGuessRequest request,
        [FromServices] SubmitGuessHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(User.GetUserId(), gameId, request, cancellationToken);
        return FromResult(result, Ok);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<GameSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> History(
        [FromServices] GetGameHistoryHandler handler,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = GetGameHistoryHandler.DefaultPageSize)
    {
        var result = await handler.HandleAsync(User.GetUserId(), page, pageSize, cancellationToken);
        return FromResult(result, Ok);
    }

    [HttpGet("{gameId:guid}")]
    [ProducesResponseType(typeof(GameSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid gameId,
        [FromServices] GetGameByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(User.GetUserId(), gameId, cancellationToken);
        return FromResult(result, Ok);
    }

    [HttpDelete("{gameId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid gameId,
        [FromServices] DeleteGameHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(User.GetUserId(), gameId, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
