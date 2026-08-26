using DSC.Application.Commands;
using DSC.Application.DTOs;
using DSC.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DSC.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(
    LoginCommandHandler loginHandler,
    LoginWithUsernameCommandHandler loginWithUsernameHandler,
    ICurrentUser currentUser,
    IConfiguration configuration
) : ControllerBase
{
    [HttpPost("login")]
    [HttpPost("login/email")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequestDto loginRequest,
        CancellationToken cancellationToken
    )
    {
        var result = await loginHandler.HandleAsync(loginRequest, cancellationToken);
        var secureCookie =
            HttpContext.Request.IsHttps || configuration.GetValue("Jwt:SecureCookie", true);

        Response.Cookies.Append(
            "access_token",
            result.Token,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = secureCookie,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(60),
            }
        );

        return Ok(result.User);
    }

    [HttpPost("login/username")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginWithUsername(
        [FromBody] LoginWithUsernameRequestDto loginRequest,
        CancellationToken cancellationToken
    )
    {
        var result = await loginWithUsernameHandler.HandleAsync(loginRequest, cancellationToken);
        var secureCookie =
            HttpContext.Request.IsHttps || configuration.GetValue("Jwt:SecureCookie", true);

        Response.Cookies.Append(
            "access_token",
            result.Token,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = secureCookie,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(60),
            }
        );

        return Ok(result.User);
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me() =>
        Ok(
            new UserProfileDto(
                currentUser.Id!.Value,
                currentUser.Username ?? string.Empty,
                currentUser.Email!,
                currentUser.Roles.ToArray()
            )
        );
}
