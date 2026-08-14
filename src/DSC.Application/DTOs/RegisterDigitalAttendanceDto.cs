namespace DSC.Application.DTOs;

public record class RegisterDigitalAttendanceDto(
    string SessionToken,
    string CiNumber,
    string Extension,
    string FirstName,
    string LastName,
    string Phone
);
