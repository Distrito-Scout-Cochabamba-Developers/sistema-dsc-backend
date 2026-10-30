using DSC.Domain.Exceptions;
using DSC.Domain.Repositories;

namespace DSC.Application.Commands;

public record class DeleteTrainingModuleCommand(string Code);

public class DeleteTrainingModuleCommandHandler(ITrainingModuleRepository moduleRepository)
{
    public async Task HandleAsync(
        DeleteTrainingModuleCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var code = command.Code.Trim();
        if (string.IsNullOrWhiteSpace(code))
            throw new RequestValidationException("El código de la ruta es obligatorio.");

        var module = await moduleRepository.GetTrackedByCodeAsync(code, cancellationToken);
        if (module is null)
            throw new KeyNotFoundException(
                $"No se encontró ningún módulo de capacitación con el código '{code}'."
            );

        if (await moduleRepository.HasDependenciesAsync(module.Id, cancellationToken))
            throw new InvalidOperationException(
                $"No se puede eliminar el módulo '{code}' porque tiene asistencia, sesiones o plantillas asociadas."
            );

        await moduleRepository.RemoveAsync(module, cancellationToken);
    }
}
