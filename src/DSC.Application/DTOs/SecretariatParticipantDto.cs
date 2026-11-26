namespace DSC.Application.DTOs;

public record SecretariatParticipantDto(
    long PersonId,
    string FirstName,
    string LastName,
    string CiNumber,
    string Extension,
    string Phone,
    bool HasAccount,
    bool IsScoutLeader,
    bool IsProfileComplete,
    string? Condition = null,
    bool? IsEnrolled = null
);
