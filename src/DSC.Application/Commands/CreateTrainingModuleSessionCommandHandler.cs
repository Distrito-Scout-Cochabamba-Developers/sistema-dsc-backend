using DSC.Application.DTOs;
using DSC.Domain.Entities;
using DSC.Domain.Exceptions;
using DSC.Domain.Repositories;

namespace DSC.Application.Commands;

public record class CreateTrainingModuleSessionCommand(
    long TrainingModuleId,
    WriteTrainingModuleSessionDto Body
);

public class CreateTrainingModuleSessionCommandHandler(
    ITrainingModuleRepository moduleRepository,
    ITrainingModuleSessionRepository sessionRepository
)
{
    public async Task<CalendarSessionDto> HandleAsync(
        CreateTrainingModuleSessionCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var module =
            await moduleRepository.GetByIdAsync(command.TrainingModuleId, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"No se encontró ningún módulo de capacitación con el id '{command.TrainingModuleId}'."
            );

        var schedules = NormalizeSchedules(command.Body.Schedules);
        var statusCode = string.IsNullOrWhiteSpace(command.Body.StatusCode)
            ? CalendarSessionMapping.DefaultSessionStatusCode
            : command.Body.StatusCode.Trim();

        var status = await sessionRepository.GetOrCreateStatusByCodeAsync(
            statusCode,
            StatusDisplayName(statusCode),
            cancellationToken
        );

        var session = new TrainingModuleSession
        {
            TrainingModuleId = module.Id,
            TrainingModuleSessionStatusId = status.Id,
            IsRegistrationOpen = command.Body.IsRegistrationOpen ?? true,
            QrToken = Guid.NewGuid(),
        };

        foreach (var block in schedules)
        {
            if (block.FacilityId is long facilityId)
            {
                _ =
                    await sessionRepository.GetFacilityByIdAsync(facilityId, cancellationToken)
                    ?? throw new KeyNotFoundException(
                        $"No se encontró la sede con el id '{facilityId}'."
                    );
            }

            session.Schedules.Add(
                new TrainingModuleSchedule
                {
                    SessionDate = block.SessionDate,
                    StartTime = block.StartTime,
                    EndTime = block.EndTime,
                    FacilityId = block.FacilityId,
                    PartNumber = block.PartNumber,
                    PartTitle = string.IsNullOrWhiteSpace(block.PartTitle)
                        ? null
                        : block.PartTitle.Trim(),
                }
            );
        }

        await sessionRepository.AddSessionAsync(session, cancellationToken);

        var created =
            await sessionRepository.GetByIdForCalendarAsync(session.Id, cancellationToken)
            ?? throw new InvalidOperationException(
                "La convocatoria se creó pero no se pudo recargar."
            );

        return CalendarSessionMapping.ToDto(created, currentPersonId: null);
    }

    private static IReadOnlyList<WriteTrainingModuleScheduleDto> NormalizeSchedules(
        IReadOnlyList<WriteTrainingModuleScheduleDto>? schedules
    )
    {
        if (schedules is null || schedules.Count == 0)
            throw new RequestValidationException(
                "La convocatoria debe tener al menos un bloque de horario."
            );

        foreach (var block in schedules)
        {
            if (block.EndTime <= block.StartTime)
                throw new RequestValidationException(
                    "La hora de fin debe ser posterior a la hora de inicio."
                );
        }

        return schedules;
    }

    private static string StatusDisplayName(string code) =>
        code.ToUpperInvariant() switch
        {
            "PLANNED" => "Planificada",
            "OPEN" => "Inscripción abierta",
            "IN_PROGRESS" => "En curso",
            "CLOSED" => "Cerrada",
            "CANCELLED" => "Cancelada",
            _ => code,
        };
}
