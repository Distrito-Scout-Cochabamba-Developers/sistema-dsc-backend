namespace DSC.Application.DTOs;

/// <summary>
/// Token emitido y su instante exacto de expiración.
/// </summary>
public record JwtTokenResult(string Token, DateTimeOffset ExpiresAt);
