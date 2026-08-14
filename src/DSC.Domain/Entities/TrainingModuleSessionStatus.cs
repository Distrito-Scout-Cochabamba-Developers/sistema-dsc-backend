namespace DSC.Domain.Entities;

/// <summary>
/// Clasificador del estado del dictado del módulo de formación (PLANNED, OPEN, IN_PROGRESS, CLOSED, CANCELLED).
/// </summary>
public class TrainingModuleSessionStatus
{
    public short Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Colecciones de navegación
    public ICollection<TrainingModuleSession> Sessions { get; set; } = [];
    public ICollection<TrainingModuleSessionStatusHistory> StatusHistories { get; set; } = [];
}
