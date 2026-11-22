using DSC.Application.DTOs;
using DSC.Application.Interfaces;
using DSC.Domain.Entities;
using DSC.Domain.Exceptions;
using DSC.Domain.Repositories;

namespace DSC.Application.Commands;

public record class EnrollInCalendarSessionCommand(long SessionId);

public class EnrollInCalendarSessionCommandHandler(
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
        var session =
            await sessionRepository.GetByIdForCalendarAsync(command.SessionId, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"No se encontró ninguna convocatoria con el id '{command.SessionId}'."
            );

        EnsureCanEnroll(session, personId);

        var source = await sessionRepository.GetOrCreateEnrollmentSourceByCodeAsync(
            CalendarSessionMapping.AuthenticatedEnrollmentSourceCode,
            "Usuario autenticado",
            cancellationToken
        );

        var enrollment = new TrainingModuleEnrollment
        {
            TrainingModuleSessionId = session.Id,
            PersonId = personId,
            EnrollmentSourceId = source.Id,
            RegisteredByUserId = currentUser.Id,
        };

        await sessionRepository.AddEnrollmentAsync(enrollment, cancellationToken);

        var updated =
            await sessionRepository.GetByIdForCalendarAsync(session.Id, cancellationToken)
            ?? session;

        return new CalendarEnrollmentDto(
            enrollment.Id,
            CalendarSessionMapping.ToDto(updated, personId)
        );
    }

    private static void EnsureCanEnroll(TrainingModuleSession session, long personId)
    {
        var status = session.Status?.Code?.ToUpperInvariant();
        if (status is "CANCELLED")
            throw new InvalidOperationException("Esta convocatoria está cancelada.");

        if (status is "CLOSED")
            throw new InvalidOperationException("Esta convocatoria ya finalizó.");

        if (!session.IsRegistrationOpen)
            throw new InvalidOperationException("La inscripción de esta convocatoria está cerrada.");

        if (session.Enrollments.Any(item => item.PersonId == personId))
            throw new InvalidOperationException("Ya estás inscrito en esta sesión.");

        var dto = CalendarSessionMapping.ToDto(session, personId);
        if (dto.RemainingSeats == 0)
            throw new InvalidOperationException("El cupo de esta sesión se agotó.");
    }
}
