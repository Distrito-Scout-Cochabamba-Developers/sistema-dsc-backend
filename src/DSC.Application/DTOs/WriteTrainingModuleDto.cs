namespace DSC.Application.DTOs;

/// <summary>
/// Alta o edición de un módulo de formación.
/// En PUT, <see cref="Code"/> se ignora: el código va en la ruta.
/// </summary>
public record class WriteTrainingModuleDto(
    string Code = "",
    string Name = "",
    string? Description = null,
    string ModuleTypeCode = ""
);
