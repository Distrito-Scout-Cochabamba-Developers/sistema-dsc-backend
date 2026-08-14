namespace DSC.Domain.Entities;

/// <summary>
/// Ficha complementaria del dirigente scout en el banco de facilitadores.
/// </summary>
public class ScoutLeader
{
    public long PersonId { get; set; }
    public bool IsSpeakerEligible { get; set; }

    // Navegación
    public Person Person { get; set; } = null!;
    public ICollection<ScoutLeaderTrainingLevel> TrainingLevels { get; set; } = [];
}
