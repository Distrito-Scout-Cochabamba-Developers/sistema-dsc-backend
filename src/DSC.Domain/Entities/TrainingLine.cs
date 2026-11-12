namespace DSC.Domain.Entities;

/// <summary>
/// Línea de formación scout (PJ, GI, AE: Adjuntos/Asesores de Formación).
/// </summary>
public class TrainingLine
{
    public short Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Colecciones de navegación
    public ICollection<TrainingLevel> TrainingLevels { get; set; } = [];
    public ICollection<TrainingModuleLine> ModuleLines { get; set; } = [];
}
