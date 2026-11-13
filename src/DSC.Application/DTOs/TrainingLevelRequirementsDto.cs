using System.ComponentModel.DataAnnotations;
using DSC.Domain.Entities;

namespace DSC.Application.DTOs;

public record TrainingLevelDto(string Code, string Name, string TrainingLineCode, bool IsActive);

public record MandatoryModuleDto(long Id, string Name, string? Description);

public record WriteTrainingLevelRequirementsDto(
    [Required] int? TotalModulesCount,
    [Required] int? MinMandatoryModulesCount,
    [Required] IReadOnlyList<long>? MandatoryModuleIds
);

public record ModuleRequirementsDto(
    int TotalModulesCount,
    int MinMandatoryModulesCount,
    IReadOnlyList<MandatoryModuleDto> MandatoryModules
);

public record TrainingLevelRequirementsDto(string TrainingLevelCode, ModuleRequirementsDto? Modules)
{
    public static TrainingLevelRequirementsDto From(TrainingLevel level)
    {
        var requirement = level.Requirements.SingleOrDefault(requirement =>
            requirement.RequirementType == TrainingLevelRequirement.ModulesType
        );
        return new(
            level.Code,
            requirement is null
                ? null
                : new(
                    requirement.TotalModulesCount,
                    requirement.MinMandatoryModulesCount,
                    requirement
                        .MandatoryModules.Select(link => link.TrainingModule)
                        .OrderBy(module => module.Id)
                        .Select(module => new MandatoryModuleDto(
                            module.Id,
                            module.Name,
                            module.Description
                        ))
                        .ToArray()
                )
        );
    }
}
