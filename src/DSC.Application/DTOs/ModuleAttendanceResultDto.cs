namespace DSC.Application.DTOs;

public record class ModuleAttendanceResultDto(
    Guid AttendanceId,
    Guid TrainingModuleId,
    string ModuleTitle,
    Guid ScoutAdultId,
    string ScoutAdultFullName,
    string CiNumber,
    DateTime RegisteredAt,
    bool IsPartialRegistration
);
