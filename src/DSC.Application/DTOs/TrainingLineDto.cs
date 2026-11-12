namespace DSC.Application.DTOs;

/// <summary>
/// Línea de formación para el catálogo curricular.
/// </summary>
public record class TrainingLineDto(short Id, string Code, string Name, string? Description);
