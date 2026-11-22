using DSC.Application.Commands;
using DSC.Application.DTOs;
using DSC.Application.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DSC.Api.Controllers;

[ApiController]
[Tags(OpenApiTags.Calendar)]
[Route("v1/calendar")]
public class CalendarController(
    ListCalendarSessionsQueryHandler listHandler,
    EnrollInCalendarSessionCommandHandler enrollHandler
) : ControllerBase
{
    /// <summary>
    /// Lista convocatorias con fecha (bloques de <c>training_module_schedule</c>).
    /// </summary>
    [HttpGet("sessions")]
    [AllowAnonymous]
    [EndpointSummary("Lista convocatorias del calendario")]
    [EndpointDescription(
        "Público. from/to opcionales (YYYY-MM-DD) filtran por session_date. Con Bearer, isEnrolled refleja a la persona del JWT."
    )]
    [ProducesResponseType(typeof(IReadOnlyList<CalendarSessionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CalendarSessionDto>>> ListSessions(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken
    )
    {
        var sessions = await listHandler.HandleAsync(
            new ListCalendarSessionsQuery(from, to),
            cancellationToken
        );
        return Ok(sessions);
    }

    /// <summary>
    /// Inscribe a la persona del JWT en una convocatoria.
    /// </summary>
    [HttpPost("sessions/{id:long}/enrollments")]
    [Authorize]
    [EndpointSummary("Inscribe al usuario autenticado en una convocatoria")]
    [EndpointDescription(
        "Requiere JWT. Cuerpo vacío. 201 con la sesión actualizada. 409 si ya está inscrito, el cupo se agotó o la inscripción está cerrada."
    )]
    [ProducesResponseType(typeof(CalendarEnrollmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CalendarEnrollmentDto>> Enroll(
        [FromRoute] long id,
        CancellationToken cancellationToken
    )
    {
        var result = await enrollHandler.HandleAsync(
            new EnrollInCalendarSessionCommand(id),
            cancellationToken
        );
        return Created($"/api/v1/calendar/sessions/{id}/enrollments/{result.EnrollmentId}", result);
    }
}
