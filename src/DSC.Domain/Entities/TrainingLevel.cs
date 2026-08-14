namespace DSC.Domain.Entities;

/// <summary>
/// Niveles o etapas formales de calificación del esquema de formación scout (Insignia de Madera Nivel II, etc.).
/// </summary>
public class TrainingLevel
{
    public short Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public short? ModuleTypeId { get; set; }
    public short? TrainingStageId { get; set; }
    public bool IsActive { get; set; } = true;

    // Navegación
    public ModuleType? ModuleType { get; set; }
    public TrainingStage? TrainingStage { get; set; }
    public ICollection<ScoutLeaderTrainingLevel> LeaderLevels { get; set; } = [];
}
