namespace DSC.Application.DTOs;

public record class ScoutAdultDto(
    Guid Id,
    string FirstName,
    string LastName,
    string CiNumber,
    string Extension,
    string Phone,
    bool IsPartialRegistration
);
