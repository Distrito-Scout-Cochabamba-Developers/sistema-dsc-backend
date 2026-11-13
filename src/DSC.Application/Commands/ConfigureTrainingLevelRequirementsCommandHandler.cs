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
            || request.MandatoryModuleCodes is null
        )
            throw new RequestValidationException(
                "Total, mínimo obligatorio y selección son obligatorios."
            );

        var codes = request.MandatoryModuleCodes.Select(value => value?.Trim()).ToArray();
        if (codes.Any(value => string.IsNullOrEmpty(value) || value.Length > 30))
            throw new RequestValidationException(
                "Cada código de módulo debe tener entre 1 y 30 caracteres."
            );
        if (codes.Distinct(StringComparer.Ordinal).Count() != codes.Length)
            throw new RequestValidationException("Los módulos obligatorios no pueden repetirse.");

        var level =
            await repository.GetTrackedByCodeAsync(code.Trim(), cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontró el nivel '{code}'.");
        var modules = await repository.GetTrackedModulesByCodesAsync(codes!, cancellationToken);
        var found = modules.Select(module => module.Code).ToHashSet(StringComparer.Ordinal);
        if (codes.Any(value => !found.Contains(value!)))
            throw new RequestValidationException(
                "La selección contiene códigos de módulos inexistentes."
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
