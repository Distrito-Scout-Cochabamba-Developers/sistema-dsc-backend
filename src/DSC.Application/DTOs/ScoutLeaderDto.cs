namespace DSC.Application.DTOs;

/// <summary>
/// DTO con los datos básicos del dirigente scout para autocompletado y visualización.
/// </summary>
public record class ScoutLeaderDto(
    long Id,
    string FirstName,
    string LastName,
    string CiNumber,
    string Extension,
    string Phone,
    bool IsProfileComplete
);
