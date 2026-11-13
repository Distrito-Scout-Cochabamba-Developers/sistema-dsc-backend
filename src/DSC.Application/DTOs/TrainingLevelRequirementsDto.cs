using System.ComponentModel.DataAnnotations;
using DSC.Domain.Entities;

namespace DSC.Application.DTOs;

public record TrainingLevelDto(string Code, string Name, string TrainingLineCode, bool IsActive);

public record WriteTrainingLevelRequirementsDto(
    [Required] int? TotalModulesCount,
    [Required] int? MinMandatoryModulesCount,
    [Required] IReadOnlyList<string>? MandatoryModuleCodes
);

public record ModuleRequirementsDto(
    int TotalModulesCount,
    int MinMandatoryModulesCount,
    IReadOnlyList<string> MandatoryModuleCodes
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
                        .MandatoryModules.Select(link => link.TrainingModule.Code)
                        .OrderBy(code => code, StringComparer.Ordinal)
                        .ToArray()
                )
        );
    }
}
