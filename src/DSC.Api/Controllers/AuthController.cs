using DSC.Application.Commands;
using DSC.Application.DTOs;
using DSC.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DSC.Api.Controllers;

[ApiController]
[Route("v1/auth")]
public class AuthController(LoginCommandHandler loginHandler, ICurrentUser currentUser)
    : ControllerBase
{
    /// <summary>
    /// Autentica con email o nombre de usuario y devuelve un JWT Bearer.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EndpointSummary("Inicia sesión con identificador (username o email) y emite un JWT")]
    [EndpointDescription(
        "Acepta email o nombre de usuario indistintamente y contraseña. Valida hash BCrypt. Si las credenciales son válidas y la cuenta está activa, emite un token JWT Bearer con claims de usuario y roles activos vigentes."
    )]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponseDto>> Login(
        [FromBody] LoginRequestDto loginRequest,
        CancellationToken cancellationToken
    )
    {
        var result = await loginHandler.HandleAsync(loginRequest, cancellationToken);

        return Ok(new LoginResponseDto(result.Token, "Bearer", result.ExpiresAt, result.User));
    }

    /// <summary>
    /// Obtiene el perfil y roles del usuario autenticado a partir del JWT.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [EndpointSummary("Obtiene el perfil y roles del usuario autenticado")]
    [EndpointDescription(
        "Requiere cabecera 'Authorization: Bearer <token>'. Extrae los claims del JWT validado y devuelve la información del usuario actual junto a sus roles asignados."
    )]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<UserProfileDto> Me()
    {
        if (currentUser.Id is null || currentUser.PersonId is null)
            return Unauthorized();

        return Ok(
            new UserProfileDto(
                currentUser.Id.Value,
                currentUser.PersonId.Value,
                currentUser.Username ?? string.Empty,
                currentUser.Email ?? string.Empty,
                currentUser.Roles.ToArray()
            )
        );
    }
}
