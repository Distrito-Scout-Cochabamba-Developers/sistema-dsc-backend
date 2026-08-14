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
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navegación
    public TrainingModule TrainingModule { get; set; } = null!;
    public TrainingModuleSessionStatus Status { get; set; } = null!;
    public TrainingModuleLessonPlan? LessonPlan { get; set; }
    public ICollection<TrainingModuleSessionStatusHistory> StatusHistories { get; set; } = [];
    public ICollection<TrainingModuleSchedule> Schedules { get; set; } = [];
    public ICollection<TrainingModuleInstructor> Instructors { get; set; } = [];
    public ICollection<TrainingModuleEnrollment> Enrollments { get; set; } = [];
}
