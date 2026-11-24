using DSC.Domain.Entities;

namespace DSC.Application.DTOs;

internal static class CalendarSessionMapping
{
    /// <summary>Hora de pared de Cochabamba; los schedules no guardan zona.</summary>
    internal static readonly TimeSpan BoliviaOffset = TimeSpan.FromHours(-4);

    internal const string AuthenticatedEnrollmentSourceCode = "AUTHENTICATED_USER";
    internal const string DefaultSessionStatusCode = "OPEN";

    public static DateTimeOffset At(DateOnly date, TimeOnly time) =>
        new(date.ToDateTime(time), BoliviaOffset);

    public static CalendarSessionDto ToDto(TrainingModuleSession session, long? currentPersonId)
    {
        var blocks = session
            .Schedules.OrderBy(block => block.PartNumber)
            .ThenBy(block => block.SessionDate)
            .ThenBy(block => block.StartTime)
            .ThenBy(block => block.Id)
            .ToArray();

        if (blocks.Length == 0)
            throw new InvalidOperationException(
                $"La convocatoria {session.Id} no tiene bloques de horario."
            );

        var first = blocks[0];
        var last = blocks[^1];
        var facility = blocks
            .Select(block => block.Facility)
            .FirstOrDefault(item => item is not null);
        var line = session
            .TrainingModule.ModuleLines.Select(link => link.TrainingLine)
            .OrderBy(item => item.Code)
            .FirstOrDefault();

        var enrolledCount = session.Enrollments.Count;
        var capacity = session.Capacity;
        int? remaining = capacity is null ? null : Math.Max(0, capacity.Value - enrolledCount);
        var statusCode = MapStatus(session.Status?.Code, remaining);

        return new CalendarSessionDto(
            session.Id,
            session.TrainingModuleId.ToString(),
            session.TrainingModule.Name,
            line?.Code ?? string.Empty,
            line?.Name ?? string.Empty,
            At(first.SessionDate, first.StartTime),
            At(last.SessionDate, last.EndTime),
            facility?.Name ?? facility?.PhysicalAddress,
            statusCode,
            capacity,
            enrolledCount,
            remaining,
            currentPersonId is long personId
                && session.Enrollments.Any(enrollment => enrollment.PersonId == personId)
        );
    }

    internal static string MapStatus(string? catalogCode, int? remainingSeats)
    {
        if (
            remainingSeats == 0
            && !string.Equals(catalogCode, "CANCELLED", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(catalogCode, "CLOSED", StringComparison.OrdinalIgnoreCase)
        )
            return "FULL";

        return catalogCode?.ToUpperInvariant() switch
        {
            "PLANNED" => "SCHEDULED",
            "OPEN" => "OPEN",
            "IN_PROGRESS" => "IN_PROGRESS",
            "CLOSED" => "FINISHED",
            "CANCELLED" => "CANCELLED",
            _ => "SCHEDULED",
        };
    }
}
