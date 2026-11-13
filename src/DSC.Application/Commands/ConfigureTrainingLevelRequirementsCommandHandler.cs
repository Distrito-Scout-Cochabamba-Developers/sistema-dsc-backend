using DSC.Application.DTOs;
using DSC.Domain.Entities;
using DSC.Domain.Exceptions;
using DSC.Domain.Repositories;

namespace DSC.Application.Commands;

public class ConfigureTrainingLevelRequirementsCommandHandler(ITrainingLevelRepository repository)
{
    public async Task<TrainingLevelRequirementsDto> HandleAsync(
        string code,
        WriteTrainingLevelRequirementsDto request,
        CancellationToken cancellationToken = default
    )
    {
        if (
            request is null
            || request.TotalModulesCount is null
            || request.MinMandatoryModulesCount is null
            || request.MandatoryModuleIds is null
        )
            throw new RequestValidationException(
                "Total, mínimo obligatorio y selección son obligatorios."
            );

        var ids = request.MandatoryModuleIds;
        if (ids.Any(id => id <= 0))
            throw new RequestValidationException("Cada id de módulo debe ser un número positivo.");

        if (ids.Distinct().Count() != ids.Count)
            throw new RequestValidationException("Los módulos obligatorios no pueden repetirse.");

        var level =
            await repository.GetTrackedByCodeAsync(code.Trim(), cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontró el nivel '{code}'.");

        var modules = await repository.GetTrackedModulesByIdsAsync(ids, cancellationToken);
        var found = modules.Select(module => module.Id).ToHashSet();
        if (ids.Any(id => !found.Contains(id)))
            throw new RequestValidationException(
                "La selección contiene identificadores de módulos inexistentes."
            );

        var requirement = level.Requirements.SingleOrDefault(requirement =>
            requirement.RequirementType == TrainingLevelRequirement.ModulesType
        );
        if (requirement is null)
            level.Requirements.Add(
                TrainingLevelRequirement.Create(
                    level.Id,
                    request.TotalModulesCount.Value,
                    request.MinMandatoryModulesCount.Value,
                    modules
                )
            );
        else
            requirement.ConfigureCurricularModules(
                request.TotalModulesCount.Value,
                request.MinMandatoryModulesCount.Value,
                modules
            );

        await repository.SaveChangesAsync(cancellationToken);
        return TrainingLevelRequirementsDto.From(level);
    }
}
