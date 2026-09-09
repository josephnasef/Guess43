using Guess43.Api.Security;
using Guess43.Application.Authentication;
using Guess43.Application.Authentication.Contracts;
using Guess43.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Guess43.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(RefreshCookieManager cookies) : ApiControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Authentication)]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        [FromServices] RegisterUserHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(request, cancellationToken);
        return WriteAuthResult(result, StatusCodes.Status201Created);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Authentication)]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        [FromServices] LoginHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(request, cancellationToken);
        return WriteAuthResult(result, StatusCodes.Status200OK);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Authentication)]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        [FromServices] RefreshTokenHandler handler,
        CancellationToken cancellationToken)
    {
        var rawToken = cookies.Read(Request);
        var result = await handler.HandleAsync(rawToken, cancellationToken);
        return WriteAuthResult(result, StatusCodes.Status200OK);
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(
        [FromServices] LogoutHandler handler,
        CancellationToken cancellationToken)
    {
        var rawToken = cookies.Read(Request);
        await handler.HandleAsync(rawToken, cancellationToken);
        cookies.Clear(Response);
        return NoContent();
    }

    private IActionResult WriteAuthResult(Result<AuthenticationResult> result, int successStatusCode)
    {
        if (result.IsFailure)
        {
            return Problem(result.Error);
        }

        var value = result.Value;
        cookies.Write(Response, value.RefreshToken, value.RefreshTokenExpiresAtUtc);

        var body = new AuthenticationResponse(value.AccessToken, value.AccessTokenExpiresAtUtc, value.User);
        return StatusCode(successStatusCode, body);
    }
}
