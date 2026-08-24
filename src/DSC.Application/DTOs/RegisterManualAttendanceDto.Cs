namespace DSC.Application.DTOs;

public record class RegisterManualAttendanceDto(
    long TrainingModuleId,
    string CiNumber,
    string Extension,
    string FirstName,
    string LastName,
    string Phone
);
