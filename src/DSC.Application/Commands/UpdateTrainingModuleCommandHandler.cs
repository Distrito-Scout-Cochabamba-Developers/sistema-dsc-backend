using DSC.Application.DTOs;
using DSC.Domain.Exceptions;
using DSC.Domain.Repositories;

namespace DSC.Application.Commands;

public record class UpdateTrainingModuleCommand(string Code, WriteTrainingModuleDto Body);

public class UpdateTrainingModuleCommandHandler(
    ITrainingModuleRepository moduleRepository,
    IModuleTypeRepository moduleTypeRepository
)
{
    public async Task<TrainingModuleDto> HandleAsync(
        UpdateTrainingModuleCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var code = command.Code.Trim();
        if (string.IsNullOrWhiteSpace(code))
            throw new RequestValidationException("El código de la ruta es obligatorio.");

        var name = NormalizeRequired(command.Body.Name, "El nombre es obligatorio.");
        var moduleTypeCode = NormalizeRequired(
            command.Body.ModuleTypeCode,
            "El tipo de módulo es obligatorio."
        );

        if (name.Length > 150)
            throw new RequestValidationException("El nombre no puede superar 150 caracteres.");

        var module = await moduleRepository.GetTrackedByCodeAsync(code, cancellationToken);
        if (module is null)
            throw new KeyNotFoundException(
                $"No se encontró ningún módulo de capacitación con el código '{code}'."
            );

        var moduleType = await moduleTypeRepository.GetByCodeAsync(
            moduleTypeCode,
            cancellationToken
        );
        if (moduleType is null)
            throw new KeyNotFoundException(
                $"No se encontró ningún tipo de módulo con el código '{moduleTypeCode}'."
            );

        module.Name = name;
        module.Description = NormalizeOptional(command.Body.Description);
        module.ModuleTypeId = moduleType.Id;

        await moduleRepository.SaveChangesAsync(cancellationToken);

        var updated =
            await moduleRepository.GetByCodeAsync(code, cancellationToken)
            ?? throw new InvalidOperationException("El módulo se actualizó pero no se pudo recargar.");

        return TrainingModuleMapping.ToDto(updated);
    }

    private static string NormalizeRequired(string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new RequestValidationException(message);

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
