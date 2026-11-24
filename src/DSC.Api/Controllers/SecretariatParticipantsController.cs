using DSC.Application.DTOs;
using DSC.Application.Commands;
using DSC.Application.Interfaces;
using DSC.Application.Queries;
using DSC.Domain.Exceptions;
using DSC.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DSC.Api.Controllers;

public record SecretariatParticipantLookupRequest(string Ci, string? Extension);
public record SecretariatEnrollmentRequest(string? Modality);

[ApiController]
[Authorize(Roles = "DISTRICT_SECRETARY")]
[Route("v1/secretariat/participants")]
public class SecretariatParticipantsController(
    LookupSecretariatParticipantByCiQueryHandler handler,
    ILogger<SecretariatParticipantsController> logger
) : ControllerBase
{
    /// <summary>Confirma la inscripción asistida de un dirigente existente.</summary>
    [HttpPost("{personId:long}/sessions/{sessionId:long}/enrollments")]
    public async Task<IActionResult> Enroll(long personId, long sessionId,
        [FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)] SecretariatEnrollmentRequest? request,
        [FromServices] EnrollParticipantInSessionCommandHandler enrollmentHandler,
        [FromServices] IScoutLeaderRepository people,
        [FromServices] ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        try
        {
            if (currentUser.Id is null) return Unauthorized();
            if (request?.Modality is not null and not "ROVER_WITHOUT_ACCOUNT") return BadRequest();
            var rover = request?.Modality == "ROVER_WITHOUT_ACCOUNT";
            var person = await people.GetByIdAsync(personId, cancellationToken);
            if (person is null || (!rover && person.ScoutLeader is null)) return NotFound();
            var enrollment = await enrollmentHandler.HandleAsync(
                new(sessionId, personId, "SECRETARIAT", "Inscripción asistida por Secretaría", currentUser.Id, rover), cancellationToken);
            return StatusCode(StatusCodes.Status201Created, new { enrollmentId = enrollment.Id });
        }
        catch (RoverHasAccountException) { return Problem(statusCode: 409, type: "urn:dsc:enrollment:rover-has-account", detail: "La persona ya tiene una cuenta."); }
        catch (EnrollmentConflictException ex)
        {
            var type = ex switch
            {
                AlreadyEnrolledException => "already-enrolled",
                SessionFullException => "session-full",
                _ => "enrollment-closed",
            };
            return Problem(statusCode: 409, type: $"urn:dsc:enrollment:{type}", detail: ex.Message);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { return new EmptyResult(); }
        catch (Exception ex)
        {
            logger.LogError("Falló la inscripción de Secretaría: {ExceptionType}", ex.GetType().Name);
            return Problem(statusCode: 500, detail: "No se pudo completar la inscripción.");
        }
    }
    /// <summary>Busca una persona por CI sin crear una inscripción.</summary>
    [HttpPost("lookup")]
    [EndpointSummary("Busca un participante por CI (Secretaría)")]
    [EndpointDescription(
        "Requiere el rol DISTRICT_SECRETARY. Si hay varias personas con el mismo CI, requiere una extensión que identifique una sola. No crea perfiles ni inscripciones."
    )]
    [ProducesResponseType(typeof(SecretariatParticipantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SecretariatParticipantDto>> Lookup(
        [FromBody] SecretariatParticipantLookupRequest request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var participant = await handler.HandleAsync(
                new LookupSecretariatParticipantByCiQuery(request.Ci, request.Extension),
                cancellationToken
            );
            return participant is null
                ? Problem(
                    statusCode: 404,
                    type: "urn:dsc:secretariat:participant-not-found",
                    detail: "No se encontró una persona con el CI indicado."
                )
                : Ok(participant);
        }
        catch (DomainException ex)
        {
            return Problem(statusCode: 400, detail: ex.Message);
        }
        catch (AmbiguousSecretariatCiException ex)
        {
            return Problem(statusCode: 409, detail: ex.Message);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (Exception ex)
        {
            // Exceptions from data access can contain personal data. Log the type only.
            logger.LogError("Falló la consulta de Secretaría: {ExceptionType}", ex.GetType().Name);
            return Problem(statusCode: 500, detail: "No se pudo completar la consulta.");
        }
    }
}
