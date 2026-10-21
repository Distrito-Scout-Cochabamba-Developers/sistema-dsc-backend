namespace DSC.Application.DTOs;

/// <summary>
/// Respuesta pública del endpoint de login.
/// </summary>
public record LoginResponseDto(
    string Token,
    string TokenType,
    DateTimeOffset ExpiresAt,
    UserProfileDto User
);
