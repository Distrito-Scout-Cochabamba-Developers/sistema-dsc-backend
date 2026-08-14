namespace DSC.Domain.Entities;

/// <summary>
/// Registro histórico de hitos y niveles de formación alcanzados por un dirigente (Kárdex).
/// </summary>
public class ScoutLeaderTrainingLevel
{
    public long Id { get; set; }
    public long PersonId { get; set; }
    public short TrainingLevelId { get; set; }
    public short StatusId { get; set; }
    public DateOnly StatusDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navegación
    public ScoutLeader ScoutLeader { get; set; } = null!;
    public TrainingLevel TrainingLevel { get; set; } = null!;
    public LeaderTrainingStatus Status { get; set; } = null!;
}
