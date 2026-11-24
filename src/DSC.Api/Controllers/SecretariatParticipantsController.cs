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
        [FromBody] SecretariatParticipantLookupRequest request,
        [FromServices] SecretariatEnrollment enrollmentHandler,
        [FromServices] ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        try
        {
            if (currentUser.Id is null) return Unauthorized();
            var enrollment = await enrollmentHandler.ExistingAsync(sessionId, personId, currentUser.Id.Value, request.Ci, request.Extension, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, new { enrollmentId = enrollment.Id });
        }
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
        catch (AmbiguousSecretariatCiException) { return Problem(statusCode: 409, type: "urn:dsc:secretariat:identity-changed", detail: "La identidad ya no es inequívoca. Vuelve a buscar."); }
        catch (DomainException) { return Problem(statusCode: 400, detail: "CI o complemento inválido."); }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { return new EmptyResult(); }
        catch (Exception ex)
        {
            logger.LogError("Falló la inscripción de Secretaría: {ExceptionType}", ex.GetType().Name);
            return Problem(statusCode: 500, detail: "No se pudo completar la inscripción.");
        }
    }

    /// <summary>Crea el perfil mínimo y confirma la inscripción de forma atómica.</summary>
    [HttpPost("sessions/{sessionId:long}/enrollments")]
    public async Task<IActionResult> CreateAndEnroll(long sessionId, [FromBody] NewSecretariatLeader request,
        [FromServices] SecretariatEnrollment enrollmentHandler, [FromServices] ICurrentUser currentUser, CancellationToken cancellationToken)
    {
        try
        {
            if (currentUser.Id is null) return Unauthorized();
            var result = await enrollmentHandler.CreateAsync(sessionId, currentUser.Id.Value, request, cancellationToken);
            return StatusCode(201, new { enrollmentId = result.Id });
        }
        catch (AmbiguousSecretariatCiException) { return Problem(statusCode: 409, type: "urn:dsc:secretariat:identity-changed", detail: "El CI ya tiene coincidencias. Vuelve a buscar."); }
        catch (DomainException ex) { return Problem(statusCode: 400, detail: ex.Message); }
        catch (EnrollmentConflictException ex)
        {
            var type = ex switch { AlreadyEnrolledException => "already-enrolled", SessionFullException => "session-full", _ => "enrollment-closed" };
            return Problem(statusCode: 409, type: $"urn:dsc:enrollment:{type}", detail: ex.Message);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { return new EmptyResult(); }
        catch (Exception ex) { logger.LogError("Falló la inscripción de Secretaría: {ExceptionType}", ex.GetType().Name); return Problem(statusCode: 500, detail: "No se pudo completar la inscripción."); }
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
