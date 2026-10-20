using DSC.Application.DTOs;
using DSC.Domain.Entities;
using DSC.Domain.Repositories;

namespace DSC.Application.Commands;

public record class CreateTrainingModuleCommand(
    string Code,
    string Name,
    string? Description,
    short ModuleTypeId
);

public class CreateTrainingModuleCommandHandler(ITrainingModuleRepository moduleRepository)
{
    public async Task<TrainingModuleDto> HandleAsync(
        CreateTrainingModuleCommand command,
        CancellationToken cancellationToken
    )
    {
        // 1. Validate and normalize input through the domain factory
        var module = TrainingModule.Create(
            command.Code,
            command.Name,
            command.Description,
            command.ModuleTypeId
        );

        // 2. Validate catalog rules
        await TrainingModuleCatalogRules.EnsureValidAsync(
            moduleRepository,
            module,
            excludeId: null,
            cancellationToken
        );

        // 3. Persist and reload with its module type for the response
        await moduleRepository.AddAsync(module, cancellationToken);

        var created = await moduleRepository.GetByIdAsync(module.Id, cancellationToken) ?? module;
        return TrainingModuleDto.FromEntity(created);
    }
}
