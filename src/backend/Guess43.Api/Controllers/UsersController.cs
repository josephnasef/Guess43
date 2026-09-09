using Guess43.Api.Security;
using Guess43.Application.Users;
using Guess43.Application.Users.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Guess43.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController : ApiControllerBase
{
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMe(
        [FromServices] GetProfileHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(User.GetUserId(), cancellationToken);
        return FromResult(result, Ok);
    }

    [HttpPut("me")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateMe(
        [FromBody] UpdateProfileRequest request,
        [FromServices] UpdateProfileHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(User.GetUserId(), request, cancellationToken);
        return FromResult(result, Ok);
    }

    [HttpDelete("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteMe(
        [FromServices] DeleteAccountHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(User.GetUserId(), cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
