namespace DSC.Domain.Entities;

/// <summary>
/// Calificación de una línea de formación scout (Insignia de Madera I-II, etc.).
/// </summary>
public class TrainingLevel
{
    public short Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public short TrainingLineId { get; set; }
    public bool IsActive { get; set; } = true;

    // Navegación
    public TrainingLine TrainingLine { get; set; } = null!;
    public ICollection<ScoutLeaderTrainingLevel> LeaderLevels { get; set; } = [];
    public ICollection<TrainingLevelRequirement> Requirements { get; private set; } = [];
}
