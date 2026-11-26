namespace DSC.Application.DTOs;

/// <summary>
/// Módulo de formación para catálogo de módulos y deep link de asistencia (`TrainingModule.Code`).
/// </summary>
public record class TrainingModuleDto(
    long Id,
    string Name,
    string? Description,
    IReadOnlyList<TrainingLineDto> TrainingLines
);
