using DSC.Domain.Exceptions;
using DSC.Domain.Repositories;

namespace DSC.Application.Commands;

public record class DeleteTrainingModuleCommand(long Id);

public class DeleteTrainingModuleCommandHandler(ITrainingModuleRepository moduleRepository)
{
    public async Task HandleAsync(
        DeleteTrainingModuleCommand command,
        CancellationToken cancellationToken = default
    )
    {
        if (command.Id <= 0)
            throw new RequestValidationException("El id debe ser un número positivo.");

        var module = await moduleRepository.GetTrackedByIdAsync(command.Id, cancellationToken);
        if (module is null)
            throw new KeyNotFoundException(
                $"No se encontró ningún módulo de capacitación con el id '{command.Id}'."
            );

        if (await moduleRepository.HasDependenciesAsync(module.Id, cancellationToken))
            throw new InvalidOperationException(
                $"No se puede eliminar el módulo '{module.Name}' porque tiene requisitos de nivel, asistencia, sesiones o plantillas asociadas."
            );

        await moduleRepository.RemoveAsync(module, cancellationToken);
    }
}
