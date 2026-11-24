using DSC.Application.Commands;
using DSC.Application.Interfaces;
using DSC.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DSC.Api.Controllers;

public record SecretariatNewRoverRequest(string? Ci, string? Extension, string? FirstName, string? LastName, string? Phone);

/// <summary>Alta mínima de un Rover sin cuenta junto con su inscripción asistida (#160).</summary>
[ApiController]
[Authorize(Roles = "DISTRICT_SECRETARY")]
[Route("v1/secretariat/sessions/{sessionId:long}/rover-enrollments")]
public class SecretariatRoverEnrollmentsController(
    EnrollNewRoverFromSecretariatCommandHandler handler,
    ICurrentUser currentUser,
    ILogger<SecretariatRoverEnrollmentsController> logger
) : ControllerBase
{
    [HttpPost]
    [EndpointSummary("Crea un Rover sin cuenta y lo inscribe (Secretaría)")]
    [EndpointDescription(
        "Requiere DISTRICT_SECRETARY. Perfil e inscripción se guardan juntos o ninguno. No crea cuenta, dirigente, afiliación ni rol; el operador sale del JWT."
    )]
    public async Task<IActionResult> Create(long sessionId, [FromBody] SecretariatNewRoverRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (currentUser.Id is null) return Unauthorized();
            var enrollment = await handler.HandleAsync(
                new(sessionId, request.Ci ?? "", request.Extension, request.FirstName ?? "", request.LastName ?? "", request.Phone, currentUser.Id.Value),
                cancellationToken);
            return StatusCode(StatusCodes.Status201Created, new { enrollmentId = enrollment.Id });
        }
        catch (RequestValidationException ex) { return Problem(statusCode: 400, detail: ex.Message); }
        catch (RoverPersonExistsException) { return Problem(statusCode: 409, type: "urn:dsc:enrollment:person-exists", detail: "Ya existe una persona con el CI indicado. Búsquela antes de inscribirla."); }
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
            // Las excepciones de datos pueden contener datos personales: solo el tipo.
            logger.LogError("Falló el alta de Rover desde Secretaría: {ExceptionType}", ex.GetType().Name);
            return Problem(statusCode: 500, detail: "No se pudo completar la inscripción.");
        }
    }
}
