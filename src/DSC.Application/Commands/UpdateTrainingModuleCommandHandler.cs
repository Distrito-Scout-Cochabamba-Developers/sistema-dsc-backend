using DSC.Application.DTOs;
using DSC.Domain.Entities;
using DSC.Domain.Exceptions;
using DSC.Domain.Repositories;

namespace DSC.Application.Commands;

public record class UpdateTrainingModuleCommand(string Code, WriteTrainingModuleDto Body);

public class UpdateTrainingModuleCommandHandler(
    ITrainingModuleRepository moduleRepository,
    ITrainingLineRepository trainingLineRepository
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

        if (name.Length > 150)
            throw new RequestValidationException("El nombre no puede superar 150 caracteres.");

        var module = await moduleRepository.GetTrackedByCodeAsync(code, cancellationToken);
        if (module is null)
            throw new KeyNotFoundException(
                $"No se encontró ningún módulo de capacitación con el código '{code}'."
            );

        var lines = await CreateTrainingModuleCommandHandler.ResolveLinesAsync(
            command.Body.TrainingLineCodes,
            trainingLineRepository,
            cancellationToken
        );

        module.Name = name;
        module.Description = NormalizeOptional(command.Body.Description);
        var lineIds = lines.Select(line => line.Id).ToHashSet();
        foreach (
            var link in module
                .ModuleLines.Where(link => !lineIds.Contains(link.TrainingLineId))
                .ToArray()
        )
            module.ModuleLines.Remove(link);
        foreach (
            var lineId in lineIds.Where(id =>
                !module.ModuleLines.Any(link => link.TrainingLineId == id)
            )
        )
            module.ModuleLines.Add(new TrainingModuleLine { TrainingLineId = lineId });

        await moduleRepository.SaveChangesAsync(cancellationToken);

        var updated =
            await moduleRepository.GetByCodeAsync(code, cancellationToken)
            ?? throw new InvalidOperationException(
                "El módulo se actualizó pero no se pudo recargar."
            );

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
