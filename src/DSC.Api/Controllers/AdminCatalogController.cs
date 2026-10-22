using DSC.Application.Authorization;
using DSC.Application.Commands;
using DSC.Application.DTOs;
using DSC.Application.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DSC.Api.Controllers;

[ApiController]
[Authorize(Roles = CatalogAdminRoles.PolicyRoles)]
[Tags(OpenApiTags.Modules)]
[Route("v1/admin")]
public class AdminCatalogController(
    ListTrainingModulesQueryHandler listModulesHandler,
    GetTrainingModuleByCodeQueryHandler getModuleHandler,
    ListModuleTypesQueryHandler listModuleTypesHandler,
    CreateTrainingModuleCommandHandler createHandler,
    UpdateTrainingModuleCommandHandler updateHandler,
    DeleteTrainingModuleCommandHandler deleteHandler
) : ControllerBase
{
    /// <summary>
    /// Lista el catálogo curricular para administración.
    /// </summary>
    [HttpGet("modules")]
    [EndpointSummary("Lista módulos de formación (admin / secretaría)")]
    [EndpointDescription(
        "Requiere JWT con rol ADMIN o DISTRICT_SECRETARY. Devuelve training_module ordenado por nombre, con tipo incluido. El GET anónimo de asistencia (/api/attendance/modules) no cambia."
    )]
    [ProducesResponseType(typeof(IReadOnlyList<TrainingModuleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<TrainingModuleDto>>> ListModules(
        CancellationToken cancellationToken
    )
    {
        var modules = await listModulesHandler.HandleAsync(cancellationToken);
        return Ok(modules);
    }

    /// <summary>
    /// Obtiene un módulo por código.
    /// </summary>
    [HttpGet("modules/{code}")]
    [EndpointSummary("Obtiene un módulo por código (admin / secretaría)")]
    [EndpointDescription(
        "Requiere JWT con rol ADMIN o DISTRICT_SECRETARY. El código es TrainingModule.Code (máx. 30)."
    )]
    [ProducesResponseType(typeof(TrainingModuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TrainingModuleDto>> GetModuleByCode(
        [FromRoute] string code,
        CancellationToken cancellationToken
    )
    {
        var module = await getModuleHandler.HandleAsync(
            new GetTrainingModuleByCodeQuery(code),
            cancellationToken
        );
        if (module is null)
            throw new KeyNotFoundException(
                $"No se encontró ningún módulo de capacitación con el código '{code}'."
            );

        return Ok(module);
    }

    /// <summary>
    /// Crea un módulo de formación.
    /// </summary>
    [HttpPost("modules")]
    [EndpointSummary("Crea un módulo de formación")]
    [EndpointDescription(
        "Requiere JWT con rol ADMIN o DISTRICT_SECRETARY. El código debe ser único (máx. 30). moduleTypeCode debe existir en module_type (p. ej. AE, PJ)."
    )]
    [ProducesResponseType(typeof(TrainingModuleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TrainingModuleDto>> CreateModule(
        [FromBody] WriteTrainingModuleDto body,
        CancellationToken cancellationToken
    )
    {
        var created = await createHandler.HandleAsync(body, cancellationToken);
        return CreatedAtAction(nameof(GetModuleByCode), new { code = created.Code }, created);
    }

    /// <summary>
    /// Actualiza nombre, descripción y tipo. El código de la ruta no cambia.
    /// </summary>
    [HttpPut("modules/{code}")]
    [EndpointSummary("Actualiza un módulo de formación")]
    [EndpointDescription(
        "Requiere JWT con rol ADMIN o DISTRICT_SECRETARY. El código de la URL identifica el módulo y no se modifica. El body usa name, description y moduleTypeCode."
    )]
    [ProducesResponseType(typeof(TrainingModuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TrainingModuleDto>> UpdateModule(
        [FromRoute] string code,
        [FromBody] WriteTrainingModuleDto body,
        CancellationToken cancellationToken
    )
    {
        var updated = await updateHandler.HandleAsync(
            new UpdateTrainingModuleCommand(code, body),
            cancellationToken
        );
        return Ok(updated);
    }

    /// <summary>
    /// Elimina un módulo si no tiene asistencia, sesiones ni plantillas.
    /// </summary>
    [HttpDelete("modules/{code}")]
    [EndpointSummary("Elimina un módulo de formación")]
    [EndpointDescription(
        "Requiere JWT con rol ADMIN o DISTRICT_SECRETARY. 204 si se elimina. 409 si el módulo tiene asistencia, sesiones o plantillas asociadas (FK Restrict)."
    )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteModule(
        [FromRoute] string code,
        CancellationToken cancellationToken
    )
    {
        await deleteHandler.HandleAsync(new DeleteTrainingModuleCommand(code), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Lista tipos de módulo para el formulario de alta/edición.
    /// </summary>
    [HttpGet("module-types")]
    [EndpointSummary("Lista tipos de módulo (AE, PJ, …)")]
    [EndpointDescription(
        "Requiere JWT con rol ADMIN o DISTRICT_SECRETARY. Devuelve module_type ordenado por nombre para el select del catálogo admin."
    )]
    [ProducesResponseType(typeof(IReadOnlyList<ModuleTypeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<ModuleTypeDto>>> ListModuleTypes(
        CancellationToken cancellationToken
    )
    {
        var types = await listModuleTypesHandler.HandleAsync(cancellationToken);
        return Ok(types);
    }
}
