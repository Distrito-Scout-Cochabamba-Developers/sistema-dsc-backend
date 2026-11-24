namespace DSC.Application.DTOs;

/// <summary>
/// Convocatoria para el calendario. Fecha/hora salen de <c>training_module_schedule</c>
/// (primer y último bloque). <c>moduleCode</c> es el id del módulo en texto
/// (el catálogo ya no tiene código).
/// </summary>
public record class CalendarSessionDto(
    long Id,
    string ModuleCode,
    string ModuleName,
    string ModuleTypeCode,
    string ModuleTypeName,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    string? Location,
    string Status,
    int? Capacity,
    int EnrolledCount,
    int? RemainingSeats,
    bool IsEnrolled
);

/// <summary>Respuesta 201 de inscripción digital.</summary>
public record class CalendarEnrollmentDto(long EnrollmentId, CalendarSessionDto Session);
