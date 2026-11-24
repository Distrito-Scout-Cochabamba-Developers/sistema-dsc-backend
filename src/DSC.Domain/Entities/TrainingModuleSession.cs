using DSC.Domain.Exceptions;

namespace DSC.Domain.Entities;

/// <summary>
/// Convocatoria o dictado concreto de un módulo de formación en el distrito.
/// </summary>
public class TrainingModuleSession
{
    public long Id { get; set; }
    public long TrainingModuleId { get; set; }
    public short TrainingModuleSessionStatusId { get; set; }
    public Guid QrToken { get; set; } = Guid.NewGuid();
    public bool IsRegistrationOpen { get; set; } = true;
    public int? Capacity { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navegación
    public TrainingModule TrainingModule { get; set; } = null!;
    public TrainingModuleSessionStatus Status { get; set; } = null!;
    public TrainingModuleLessonPlan? LessonPlan { get; set; }
    public ICollection<TrainingModuleSessionStatusHistory> StatusHistories { get; set; } = [];
    public ICollection<TrainingModuleSchedule> Schedules { get; set; } = [];
    public ICollection<TrainingModuleInstructor> Instructors { get; set; } = [];
    public ICollection<TrainingModuleEnrollment> Enrollments { get; set; } = [];

    /// <summary>
    /// Reglas de ventana, duplicado y cupo propio de la convocatoria. Debe
    /// invocarse sobre una lectura bloqueada y con <see cref="Enrollments"/>
    /// cargadas; un duplicado se evalúa antes que el cupo para que no consuma
    /// una plaza.
    /// </summary>
    public void EnsureCanEnroll(long personId)
    {
        var status = Status?.Code?.ToUpperInvariant();
        if (status is "CANCELLED")
            throw new SessionCancelledException();

        if (status is "CLOSED")
            throw new SessionFinishedException();

        if (!IsRegistrationOpen)
            throw new EnrollmentClosedException();

        if (Enrollments.Any(enrollment => enrollment.PersonId == personId))
            throw new AlreadyEnrolledException();

        if (Capacity is int capacity && Enrollments.Count >= capacity)
            throw new SessionFullException();
    }
}
