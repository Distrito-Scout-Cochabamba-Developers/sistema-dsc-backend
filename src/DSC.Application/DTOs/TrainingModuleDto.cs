namespace DSC.Application.DTOs;

/// <summary>
/// Módulo de formación para catálogo y deep link de asistencia (`TrainingModule.Code`).
/// </summary>
public record class TrainingModuleDto(
    long Id,
    string Code,
    string Name,
    string? Description,
    string ModuleTypeCode,
    string ModuleTypeName
);
