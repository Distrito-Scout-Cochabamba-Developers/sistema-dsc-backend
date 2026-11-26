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
    GetTrainingModuleByIdQueryHandler getModuleHandler,
    ListTrainingLinesQueryHandler listTrainingLinesHandler,
    CreateTrainingModuleCommandHandler createHandler,
    UpdateTrainingModuleCommandHandler updateHandler,
    DeleteTrainingModuleCommandHandler deleteHandler,
    CreateTrainingModuleSessionCommandHandler createSessionHandler
) : ControllerBase
{
    /// <summary>
    /// Lista el catálogo de módulos curricular para administración.
    /// </summary>
    [HttpGet("modules")]
    [EndpointSummary("Lista módulos de formación (admin / secretaría)")]
    [EndpointDescription(
        "Requiere JWT con rol ADMIN o DISTRICT_SECRETARY. Devuelve training_module ordenado por nombre, con sus líneas de formación. Permite filtro por búsqueda en el nombre."
    )]
    [ProducesResponseType(typeof(IReadOnlyList<TrainingModuleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<TrainingModuleDto>>> ListModules(
        [FromQuery] string? search,
        CancellationToken cancellationToken
    )
    {
        var modules = await listModulesHandler.HandleAsync(
            new ListTrainingModulesQuery(search),
            cancellationToken
        );
        return Ok(modules);
    }

    /// <summary>
    /// Obtiene un módulo por id.
    /// </summary>
    [HttpGet("modules/{id:long}")]
    [EndpointSummary("Obtiene un módulo por id (admin / secretaría)")]
    [EndpointDescription(
        "Requiere JWT con rol ADMIN o DISTRICT_SECRETARY. El id identifica el registro en training_module."
    )]
    [ProducesResponseType(typeof(TrainingModuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TrainingModuleDto>> GetModuleById(
        [FromRoute] long id,
        CancellationToken cancellationToken
    )
    {
        var module = await getModuleHandler.HandleAsync(
            new GetTrainingModuleByIdQuery(id),
            cancellationToken
        );
        if (module is null)
            throw new KeyNotFoundException(
                $"No se encontró ningún módulo de capacitación con el id '{id}'."
            );

        return Ok(module);
    }

    /// <summary>
    /// Crea un módulo de formación.
    /// </summary>
    [HttpPost("modules")]
    [EndpointSummary("Crea un módulo de formación")]
    [EndpointDescription(
        "Requiere JWT con rol ADMIN o DISTRICT_SECRETARY. El nombre debe ser único (máx. 150, case-insensitive). trainingLineCodes debe contener al menos un código existente en training_line, sin duplicados (p. ej. AE, PJ)."
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
        return CreatedAtAction(nameof(GetModuleById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Actualiza nombre, descripción y líneas. El id de la ruta no cambia.
    /// </summary>
    [HttpPut("modules/{id:long}")]
    [EndpointSummary("Actualiza un módulo de formación")]
    [EndpointDescription(
        "Requiere JWT con rol ADMIN o DISTRICT_SECRETARY. El id de la URL identifica el módulo y no se modifica. El body usa name, description y trainingLineCodes; reemplaza las líneas actuales por al menos una línea existente, sin duplicados."
    )]
    [ProducesResponseType(typeof(TrainingModuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TrainingModuleDto>> UpdateModule(
        [FromRoute] long id,
        [FromBody] WriteTrainingModuleDto body,
        CancellationToken cancellationToken
    )
    {
        var updated = await updateHandler.HandleAsync(
            new UpdateTrainingModuleCommand(id, body),
            cancellationToken
        );
        return Ok(updated);
    }

    /// <summary>
    /// Elimina un módulo si no tiene requisitos de nivel, asistencia, sesiones ni plantillas.
    /// </summary>
    [HttpDelete("modules/{id:long}")]
    [EndpointSummary("Elimina un módulo de formación")]
    [EndpointDescription(
        "Requiere JWT con rol ADMIN o DISTRICT_SECRETARY. 204 si se elimina. 409 si el módulo tiene requisitos de nivel, asistencia, sesiones o plantillas asociadas (FK Restrict)."
    )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteModule(
        [FromRoute] long id,
        CancellationToken cancellationToken
    )
    {
        await deleteHandler.HandleAsync(new DeleteTrainingModuleCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Lista líneas de formación para el formulario de alta/edición.
    /// </summary>
    /// <summary>
    /// Crea una programación con bloques de horario para un módulo del catálogo de módulos.
    /// </summary>
    [HttpPost("modules/{id:long}/sessions")]
    [EndpointSummary("Crea una programación de un módulo")]
    [EndpointDescription(
        "Requiere JWT con rol ADMIN o DISTRICT_SECRETARY. Fecha y horas van en schedules (training_module_schedule). statusCode por defecto OPEN. La sede (facilityId) es opcional."
    )]
    [ProducesResponseType(typeof(CalendarSessionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CalendarSessionDto>> CreateModuleSession(
        [FromRoute] long id,
        [FromBody] WriteTrainingModuleSessionDto body,
        CancellationToken cancellationToken
    )
    {
        var created = await createSessionHandler.HandleAsync(
            new CreateTrainingModuleSessionCommand(
                id,
                body,
                User.IsInRole(CatalogAdminRoles.Admin)
            ),
            cancellationToken
        );
        return Created($"/api/v1/calendar/sessions/{created.Id}", created);
    }

    [HttpPut("sessions/{id:long}/capacity")]
    [Authorize(Roles = CatalogAdminRoles.Admin)]
    [EndpointSummary("Amplía el cupo de una programación (solo ADMIN)")]
    [EndpointDescription(
        "capacity es obligatorio: entero no negativo para ampliar o null para quitar el límite."
    )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ExpandSessionCapacity(
        [FromRoute] long id,
        [FromBody] ExpandTrainingModuleSessionCapacityDto body,
        [FromServices] ExpandTrainingModuleSessionCapacityCommandHandler handler,
        CancellationToken cancellationToken
    )
    {
        await handler.HandleAsync(id, body.Capacity, cancellationToken);
        return NoContent();
    }

    [HttpGet("training-lines")]
    [EndpointSummary("Lista líneas de formación (AE, PJ, GI)")]
    [EndpointDescription(
        "Requiere JWT con rol ADMIN o DISTRICT_SECRETARY. Devuelve training_line ordenado por id para el catálogo de módulos admin."
    )]
    [ProducesResponseType(typeof(IReadOnlyList<TrainingLineDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<TrainingLineDto>>> ListTrainingLines(
        CancellationToken cancellationToken
    )
    {
        var lines = await listTrainingLinesHandler.HandleAsync(cancellationToken);
        return Ok(lines);
    }
}
