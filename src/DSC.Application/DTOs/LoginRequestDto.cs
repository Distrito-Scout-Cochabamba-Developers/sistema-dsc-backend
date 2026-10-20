namespace DSC.Application.DTOs;

/// <summary>
/// Credenciales de acceso. El identificador acepta email o nombre de usuario.
/// </summary>
public record LoginRequestDto(string Identifier = "", string Password = "");
