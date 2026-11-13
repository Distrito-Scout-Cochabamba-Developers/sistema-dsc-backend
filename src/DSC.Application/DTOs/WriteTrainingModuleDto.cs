namespace DSC.Application.DTOs;

/// <summary>
/// Alta o edición de un módulo de formación.
/// </summary>
public record class WriteTrainingModuleDto(
    string Name = "",
    string? Description = null,
    IReadOnlyList<string>? TrainingLineCodes = null
);
