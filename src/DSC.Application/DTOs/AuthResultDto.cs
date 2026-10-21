namespace DSC.Application.DTOs;

public record AuthResultDto(string Token, DateTimeOffset ExpiresAt, UserProfileDto User);
