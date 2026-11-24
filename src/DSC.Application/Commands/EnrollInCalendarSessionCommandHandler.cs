using DSC.Application.DTOs;
using DSC.Application.Interfaces;
using DSC.Domain.Exceptions;
using DSC.Domain.Repositories;

namespace DSC.Application.Commands;

public record class EnrollInCalendarSessionCommand(long SessionId);

/// <summary>
/// Adaptador de autoservicio sobre la operación compartida de inscripción.
/// El contrato HTTP se conserva: 201 con la sesión actualizada y 409 para
/// duplicado, cupo agotado o ventana cerrada.
/// </summary>
public class EnrollInCalendarSessionCommandHandler(
    EnrollParticipantInSessionCommandHandler enrollmentHandler,
    ITrainingModuleSessionRepository sessionRepository,
    ICurrentUser currentUser
)
{
    public async Task<CalendarEnrollmentDto> HandleAsync(
        EnrollInCalendarSessionCommand command,
        CancellationToken cancellationToken = default
    )
    {
        if (currentUser.PersonId is null || currentUser.Id is null)
            throw new RequestValidationException(
                "La cuenta autenticada no tiene una persona asociada."
            );

        var personId = currentUser.PersonId.Value;
        var enrollment = await enrollmentHandler.HandleAsync(
            new EnrollParticipantInSessionCommand(
                command.SessionId,
                personId,
                CalendarSessionMapping.AuthenticatedEnrollmentSourceCode,
                CalendarSessionMapping.AuthenticatedEnrollmentSourceName,
                currentUser.Id,
                false
            ),
            cancellationToken
        );

        var updated =
            await sessionRepository.GetByIdForCalendarAsync(command.SessionId, cancellationToken)
            ?? throw new InvalidOperationException(
                "La inscripción se guardó pero no se pudo recargar la convocatoria."
            );

        return new CalendarEnrollmentDto(
            enrollment.Id,
            CalendarSessionMapping.ToDto(updated, personId)
        );
    }
}
