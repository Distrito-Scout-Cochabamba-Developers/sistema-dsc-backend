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
            module
                .ModuleLines.Select(link => link.TrainingLine)
                .OrderBy(line => line.Code)
                .Select(line => new TrainingLineDto(
                    line.Id,
                    line.Code,
                    line.Name,
                    line.Description
                ))
                .ToArray()
        );
}
