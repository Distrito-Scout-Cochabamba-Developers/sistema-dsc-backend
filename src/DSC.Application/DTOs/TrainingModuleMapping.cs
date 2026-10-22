using DSC.Domain.Entities;

namespace DSC.Application.DTOs;

internal static class TrainingModuleMapping
{
    public static TrainingModuleDto ToDto(TrainingModule module) =>
        new(
            module.Id,
            module.Code,
            module.Name,
            module.Description,
            module.ModuleType?.Code ?? string.Empty,
            module.ModuleType?.Name ?? string.Empty
        );
}
