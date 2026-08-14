namespace DSC.Domain.Entities;

/// <summary>
/// Horario, fecha y locación/ambiente (Facility) asignado al dictado del módulo de formación.
/// </summary>
public class TrainingModuleSchedule
{
    public long Id { get; set; }
    public long TrainingModuleSessionId { get; set; }
    public long? FacilityId { get; set; }
    public short PartNumber { get; set; } = 1;
    public string? PartTitle { get; set; }
    public DateOnly SessionDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    // Navegación
    public TrainingModuleSession Session { get; set; } = null!;
    public Facility? Facility { get; set; }
}
