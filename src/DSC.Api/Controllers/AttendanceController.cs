using DSC.Application.Commands;
using DSC.Application.DTOs;
using DSC.Application.Queries;
using Microsoft.AspNetCore.Mvc;

namespace DSC.Api.Controllers;

[ApiController]
[Route("attendance")]
public class AttendanceController(
    LookupLeaderByCiQueryHandler lookupHandler,
    ListTrainingModulesQueryHandler listModulesHandler,
    GetTrainingModuleByCodeQueryHandler getModuleHandler,
    RegisterDigitalAttendanceCommandHandler registerHandler
) : ControllerBase
{
    /// <summary>
    /// Lista el catálogo de módulos de formación para la pantalla de inscripción.
    /// </summary>
    [HttpGet("modules")]
    [ProducesResponseType(typeof(IReadOnlyList<TrainingModuleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListModules(CancellationToken cancellationToken)
    {
        var modules = await listModulesHandler.HandleAsync(
            new ListTrainingModulesQuery(),
            cancellationToken
        );
        return Ok(modules);
    }

    /// <summary>
    /// Obtiene un módulo por código (el mismo token del deep link / QR de asistencia).
    /// </summary>
    [HttpGet("modules/{code}")]
    [ProducesResponseType(typeof(TrainingModuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetModuleByCode(
        [FromRoute] string code,
        CancellationToken cancellationToken
    )
    {
        var module = await getModuleHandler.HandleAsync(
            new GetTrainingModuleByCodeQuery(code),
            cancellationToken
        );
        if (module is null)
            return NotFound(
                new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Módulo No Encontrado",
                    Detail = $"No se encontró ningún módulo de capacitación con el código '{code}'.",
                }
            );

        return Ok(module);
    }

    /// <summary>
    /// Busca un dirigente scout por número de CI para autocompletar el formulario de asistencia.
    /// </summary>
    [HttpGet("lookup-ci/{ci}")]
    [ProducesResponseType(typeof(ScoutLeaderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LookupByCi(
        [FromRoute] string ci,
        [FromQuery] string extension = "",
        CancellationToken cancellationToken = default
    )
    {
        var result = await lookupHandler.HandleAsync(
            new LookupLeaderByCiQuery(ci, extension),
            cancellationToken
        );

        if (result == null)
            return NotFound(
                new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Dirigente No Encontrado",
                    Detail = $"No se encontró ningún dirigente registrado con el CI '{ci}'.",
                }
            );

        return Ok(result);
    }

    /// <summary>
    /// Registra la asistencia digital de un dirigente scout a una sesión de módulo mediante QR.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ModuleAttendanceResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterAttendance(
        [FromBody] RegisterDigitalAttendanceDto request,
        CancellationToken cancellationToken = default
    )
    {
        var result = await registerHandler.HandleAsync(request, cancellationToken);
        return CreatedAtAction(nameof(LookupByCi), new { ci = request.CiNumber }, result);
    }
}
