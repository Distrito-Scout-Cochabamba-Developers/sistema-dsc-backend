namespace DSC.Domain.Entities;

/// <summary>
/// Clasificador del avance o hito de un dirigente en un nivel formativo (ATTENDED, APPROVED, APPOINTED).
/// </summary>
public class LeaderTrainingStatus
{
    public short Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Colecciones de navegación
    public ICollection<ScoutLeaderTrainingLevel> LeaderTrainingLevels { get; set; } = [];
}
