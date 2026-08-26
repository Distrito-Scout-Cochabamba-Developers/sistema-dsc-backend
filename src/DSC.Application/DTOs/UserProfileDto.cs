namespace DSC.Application.DTOs;

public record UserProfileDto(
    long Id,
    string Username,
    string Email,
    IReadOnlyCollection<string> Roles
);
