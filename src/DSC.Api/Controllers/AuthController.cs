using DSC.Application.Commands;
using DSC.Application.DTOs;
using DSC.Application.Interfaces;
using DSC.Application.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DSC.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(LoginCommandHandler loginHandler, ICurrentUser _currentUser)
    : ControllerBase
{
    private readonly ICurrentUser _user = _currentUser;

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequestDto loginRequest,
        CancellationToken cancellationToken
    )
    {
        var result = await loginHandler.HandleAsync(loginRequest, cancellationToken);
        var secureCookie =
            HttpContext.Request.IsHttps
            || HttpContext
                .RequestServices.GetRequiredService<IConfiguration>()
                .GetValue("Jwt:SecureCookie", true);

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
        Ok(new UserProfileDto(_user.Id!.Value, _user.Email!, _user.Roles.ToArray()));
}
