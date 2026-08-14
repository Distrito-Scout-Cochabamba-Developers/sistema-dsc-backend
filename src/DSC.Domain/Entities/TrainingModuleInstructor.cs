namespace DSC.Domain.Entities;

/// <summary>
/// Disertante o formador asignado al dictado de una sesión concreta de módulo.
/// </summary>
public class TrainingModuleInstructor
{
    public long Id { get; set; }
    public long TrainingModuleSessionId { get; set; }
    public long PersonId { get; set; }
    public DateTime InvitedAt { get; set; } = DateTime.UtcNow;
    public bool IsConfirmed { get; set; }
    public DateTime? ConfirmedAt { get; set; }

    // Navegación
    public TrainingModuleSession Session { get; set; } = null!;
    public Person Person { get; set; } = null!;
    public ICollection<TrainingModuleEvaluation> Evaluations { get; set; } = [];
}
