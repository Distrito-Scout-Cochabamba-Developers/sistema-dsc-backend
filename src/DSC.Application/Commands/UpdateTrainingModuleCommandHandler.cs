using DSC.Application.DTOs;
using DSC.Domain.Entities;
using DSC.Domain.Repositories;

namespace DSC.Application.Commands;

public record class UpdateTrainingModuleCommand(
    long Id,
    string Code,
    string Name,
    string? Description,
    short ModuleTypeId,
    bool IsActive
);

public class UpdateTrainingModuleCommandHandler(ITrainingModuleRepository moduleRepository)
{
    public async Task<TrainingModuleDto> HandleAsync(
        UpdateTrainingModuleCommand command,
        CancellationToken cancellationToken
    )
    {
        // 1. Load the tracked module
        var module =
            await moduleRepository.GetByIdForUpdateAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"No se encontró ningún módulo de capacitación con el identificador '{command.Id}'."
            );

        // 2. Validate and normalize on a detached candidate so the tracked entity stays untouched on failure
        var candidate = TrainingModule.Create(
            command.Code,
            command.Name,
            command.Description,
            command.ModuleTypeId
        );
        candidate.IsActive = command.IsActive;

        await TrainingModuleCatalogRules.EnsureValidAsync(
            moduleRepository,
            candidate,
            excludeId: module.Id,
            cancellationToken
        );

        // 3. Apply and persist
        module.Update(
            candidate.Code,
            candidate.Name,
            candidate.Description,
            candidate.ModuleTypeId,
            candidate.IsActive
        );
        await moduleRepository.SaveChangesAsync(cancellationToken);

        var updated = await moduleRepository.GetByIdAsync(module.Id, cancellationToken) ?? module;
        return TrainingModuleDto.FromEntity(updated);
    }
}
