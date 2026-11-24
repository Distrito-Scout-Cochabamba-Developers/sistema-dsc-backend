using DSC.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DSC.Api.Controllers;

/// <summary>
/// Invitaciones de activación de cuentas creadas por la inscripción asistida. El reintento es solo manual.
/// Las respuestas no contienen correo, usuario, enlace ni CI.
/// </summary>
[ApiController]
[Authorize(Roles = "DISTRICT_SECRETARY")]
[Route("v1/secretariat/invitations")]
public class SecretariatInvitationsController(IAccountInvitationDelivery delivery, ILogger<SecretariatInvitationsController> logger) : ControllerBase
{
    /// <summary>Invitaciones aún no entregadas (sin datos personales).</summary>
    [HttpGet("pending")]
    public async Task<IActionResult> Pending(CancellationToken cancellationToken)
    {
        Response.Headers.CacheControl = "no-store";
        return Ok(await delivery.ListPendingAsync(cancellationToken));
    }

    /// <summary>Reintenta manualmente la entrega. No modifica la inscripción ni crea otra.</summary>
    [HttpPost("{enrollmentId:long}/retry")]
    public async Task<IActionResult> Retry(long enrollmentId, CancellationToken cancellationToken)
    {
        Response.Headers.CacheControl = "no-store";
        try
        {
            return await delivery.DeliverAsync(enrollmentId, cancellationToken) switch
            {
                InvitationDeliveryOutcome.Sent => Ok(new { invitation = "SENT" }),
                InvitationDeliveryOutcome.Pending => Problem(statusCode: 502, type: "urn:dsc:invitation:delivery-failed", detail: "No se pudo entregar el correo. Sigue pendiente; puedes reintentar."),
                InvitationDeliveryOutcome.AlreadySent => Problem(statusCode: 409, type: "urn:dsc:invitation:already-sent", detail: "La invitación ya fue entregada."),
                InvitationDeliveryOutcome.NotEligible => Problem(statusCode: 409, type: "urn:dsc:invitation:not-eligible", detail: "La cuenta ya no admite una invitación."),
                _ => NotFound(),
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { return new EmptyResult(); }
        catch (Exception ex)
        {
            logger.LogError("Falló el reintento de invitación: {ExceptionType}", ex.GetType().Name);
            return Problem(statusCode: 500, detail: "No se pudo completar el reintento.");
        }
    }
}
