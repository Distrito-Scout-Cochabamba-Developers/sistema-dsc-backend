namespace DSC.Domain.Entities;

/// <summary>
/// Bitácora inmutable de transiciones de estado de una sesión de formación (Status History Pattern).
/// </summary>
public class TrainingModuleSessionStatusHistory
{
    public long Id { get; set; }
    public long TrainingModuleSessionId { get; set; }
    public short StatusId { get; set; }
    public long? ChangedByUserId { get; set; }
    public string? Reason { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    // Navegación
    public TrainingModuleSession Session { get; set; } = null!;
    public TrainingModuleSessionStatus Status { get; set; } = null!;
    public User? ChangedByUser { get; set; }
}
