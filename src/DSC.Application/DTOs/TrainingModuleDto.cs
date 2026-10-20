using DSC.Domain.Entities;

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
    string ModuleTypeName,
    bool IsActive
)
{
    public static TrainingModuleDto FromEntity(TrainingModule module) =>
        new(
            module.Id,
            module.Code,
            module.Name,
            module.Description,
            module.ModuleType?.Code ?? string.Empty,
            module.ModuleType?.Name ?? string.Empty,
            module.IsActive
        );
}
