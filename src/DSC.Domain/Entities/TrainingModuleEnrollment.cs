namespace DSC.Domain.Entities;

/// <summary>
/// Registro de inscripción y asistencia de un participante a un módulo de formación.
/// </summary>
public class TrainingModuleEnrollment
{
    public long Id { get; set; }
    public long TrainingModuleSessionId { get; set; }
    public long PersonId { get; set; }
    public short EnrollmentSourceId { get; set; }
    public bool Attended { get; set; }
    public DateTime? AttendanceRegisteredAt { get; set; }
    public long? RegisteredByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navegación
    public TrainingModuleSession Session { get; set; } = null!;
    public Person Person { get; set; } = null!;
    public EnrollmentSource EnrollmentSource { get; set; } = null!;
    public User? RegisteredByUser { get; set; }
    public ICollection<TrainingModuleEvaluation> GivenEvaluations { get; set; } = [];
}
