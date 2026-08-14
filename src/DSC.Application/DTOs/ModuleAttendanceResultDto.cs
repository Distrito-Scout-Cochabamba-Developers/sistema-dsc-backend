namespace DSC.Application.DTOs;

public record class ModuleAttendanceResultDto(
    Guid AttendanceId,
    long TrainingModuleId,
    string ModuleTitle,
    long ScoutLeaderId,
    string ScoutLeaderFullName,
    string CiNumber,
    DateTime RegisteredAt,
    bool IsPartialRegistration
);
