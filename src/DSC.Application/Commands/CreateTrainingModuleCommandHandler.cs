using DSC.Application.DTOs;
using DSC.Domain.Entities;
using DSC.Domain.Exceptions;
using DSC.Domain.Repositories;

namespace DSC.Application.Commands;

public class CreateTrainingModuleCommandHandler(
    ITrainingModuleRepository moduleRepository,
    ITrainingLineRepository trainingLineRepository
)
{
    public async Task<TrainingModuleDto> HandleAsync(
        WriteTrainingModuleDto request,
        CancellationToken cancellationToken = default
    )
    {
        var name = NormalizeRequired(request.Name, "El nombre es obligatorio.");

        if (name.Length > 150)
            throw new RequestValidationException("El nombre no puede superar 150 caracteres.");

        var exists = await moduleRepository.ExistsByNameAsync(name, null, cancellationToken);
        if (exists)
            throw new InvalidOperationException($"Ya existe un módulo con el nombre '{name}'.");

        var lines = await ResolveLinesAsync(
            request.TrainingLineCodes,
            trainingLineRepository,
            cancellationToken
        );

        var module = new TrainingModule
        {
            Name = name,
            Description = NormalizeOptional(request.Description),
            ModuleLines = lines
                .Select(line => new TrainingModuleLine { TrainingLineId = line.Id })
                .ToList(),
        };

        await moduleRepository.AddAsync(module, cancellationToken);

        var created =
            await moduleRepository.GetByIdAsync(module.Id, cancellationToken)
            ?? throw new InvalidOperationException("El módulo se creó pero no se pudo recargar.");

        return TrainingModuleMapping.ToDto(created);
    }

    internal static async Task<IReadOnlyList<TrainingLine>> ResolveLinesAsync(
        IReadOnlyList<string>? codes,
        ITrainingLineRepository repository,
        CancellationToken cancellationToken
    )
    {
        if (codes is null || codes.Count == 0)
            throw new RequestValidationException(
                "El módulo debe pertenecer al menos a una línea de formación."
            );

        var normalized = codes
            .Select(code => NormalizeRequired(code, "El código de línea es obligatorio."))
            .ToArray();
        if (normalized.Any(code => code.Length > 20))
            throw new RequestValidationException(
                "El código de línea no puede superar 20 caracteres."
            );
        if (normalized.Distinct(StringComparer.Ordinal).Count() != normalized.Length)
            throw new RequestValidationException("Las líneas de formación no pueden repetirse.");

        var lines = new List<TrainingLine>();
        foreach (var code in normalized)
        {
            var line =
                await repository.GetByCodeAsync(code, cancellationToken)
                ?? throw new KeyNotFoundException(
                    $"No se encontró ninguna línea de formación con el código '{code}'."
                );
            lines.Add(line);
        }
        return lines;
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
