namespace DSC.Application.DTOs;

public record UserProfileDto(
    long Id,
    long PersonId,
    string Username,
    string Email,
    IReadOnlyCollection<string> Roles
);
