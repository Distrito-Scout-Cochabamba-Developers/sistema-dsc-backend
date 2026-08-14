namespace DSC.Domain.Entities;

/// <summary>
/// Clasificador de etapas del esquema formativo scout (Formación Elemental, Perfeccionamiento Continuo).
/// </summary>
public class TrainingStage
{
    public short Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Colecciones de navegación
    public ICollection<TrainingLevel> TrainingLevels { get; set; } = [];
}
