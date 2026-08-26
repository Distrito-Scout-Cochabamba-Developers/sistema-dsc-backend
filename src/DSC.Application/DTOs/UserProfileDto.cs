namespace DSC.Application.DTOs;

public record UserProfileDto(long Id, string Email, IReadOnlyCollection<string> Roles);