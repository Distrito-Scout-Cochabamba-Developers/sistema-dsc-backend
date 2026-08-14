namespace DSC.Domain.Entities;

/// <summary>
/// Línea o tipo de formación scout (PJ: Programa de Jóvenes, GI: Gestión Institucional, AE: Adultos en el Escultismo).
/// </summary>
public class ModuleType
{
    public short Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Colecciones de navegación
    public ICollection<TrainingLevel> TrainingLevels { get; set; } = [];
    public ICollection<TrainingModule> TrainingModules { get; set; } = [];
}
