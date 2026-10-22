using DSC.Application.DTOs;
using DSC.Domain.Entities;
using DSC.Domain.Exceptions;
using DSC.Domain.Repositories;

namespace DSC.Application.Commands;

public class CreateTrainingModuleCommandHandler(
    ITrainingModuleRepository moduleRepository,
    IModuleTypeRepository moduleTypeRepository
)
{
    public async Task<TrainingModuleDto> HandleAsync(
        WriteTrainingModuleDto request,
        CancellationToken cancellationToken = default
    )
    {
        var code = NormalizeRequired(request.Code, "El código es obligatorio.");
        var name = NormalizeRequired(request.Name, "El nombre es obligatorio.");
        var moduleTypeCode = NormalizeRequired(
            request.ModuleTypeCode,
            "El tipo de módulo es obligatorio."
        );

        if (code.Length > 30)
            throw new RequestValidationException("El código no puede superar 30 caracteres.");

        if (name.Length > 150)
            throw new RequestValidationException("El nombre no puede superar 150 caracteres.");

        var existing = await moduleRepository.GetByCodeAsync(code, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException($"Ya existe un módulo con el código '{code}'.");

        var moduleType = await moduleTypeRepository.GetByCodeAsync(
            moduleTypeCode,
            cancellationToken
        );
        if (moduleType is null)
            throw new KeyNotFoundException(
                $"No se encontró ningún tipo de módulo con el código '{moduleTypeCode}'."
            );

        var module = new TrainingModule
        {
            Code = code,
            Name = name,
            Description = NormalizeOptional(request.Description),
            ModuleTypeId = moduleType.Id,
        };

        await moduleRepository.AddAsync(module, cancellationToken);

        var created =
            await moduleRepository.GetByCodeAsync(code, cancellationToken)
            ?? throw new InvalidOperationException("El módulo se creó pero no se pudo recargar.");

        return TrainingModuleMapping.ToDto(created);
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
