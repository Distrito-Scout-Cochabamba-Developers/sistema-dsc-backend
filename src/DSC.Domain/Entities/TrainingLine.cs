using DSC.Domain.ValueObjects;

namespace DSC.Domain.Entities;

/// <summary>
/// Línea de formación scout (PJ, GI, AE: Adjuntos/Asesores de Formación).
/// </summary>
public class TrainingLine
{
    public TrainingLineId Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    // Colecciones de navegación
    public ICollection<TrainingLevel> TrainingLevels { get; private set; } = [];
    public ICollection<TrainingModuleLine> ModuleLines { get; private set; } = [];

    private TrainingLine() { }

    public TrainingLine(TrainingLineId id, string code, string name, string? description = null)
    {
        Id = id;
        Code = code;
        Name = name;
        Description = description;
    }

    public TrainingLine(string code, string name, string? description = null)
        : this(default, code, name, description) { }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
    }
}
