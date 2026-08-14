using DSC.Domain.Enums;

namespace DSC.Domain.Entities;

/// <summary>
/// Representa la identidad central e intrínseca de una persona (datos biológicos y civiles).
/// </summary>
public class Person
{
    public long Id { get; set; }
    public string IdCard { get; set; } = string.Empty;
    public string? IdCardExtension { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string FirstLastName { get; set; } = string.Empty;
    public string? SecondLastName { get; set; }
    public GenderType? Gender { get; set; }
    public DateOnly? BirthDate { get; set; }
    public long? PhotoFileId { get; set; }
    public short PersonStatusId { get; set; }
    public bool IsProfileComplete { get; set; }

    /// <summary>
    /// Nombre completo computado.
    /// </summary>
    public string FullName => string.Join(" ",
        new[] { FirstName, MiddleName, FirstLastName, SecondLastName }
        .Where(s => !string.IsNullOrWhiteSpace(s)));

    // Navegación
    public FileAsset? PhotoFile { get; set; }
    public PersonStatus PersonStatus { get; set; } = null!;
    public User? User { get; set; }
    public ScoutLeader? ScoutLeader { get; set; }
    public ICollection<PersonContactMechanism> ContactMechanisms { get; set; } = [];
    public ICollection<ScoutAffiliation> Affiliations { get; set; } = [];
    public ICollection<TrainingModuleInstructor> InstructorAssignments { get; set; } = [];
    public ICollection<TrainingModuleEnrollment> Enrollments { get; set; } = [];
    public ICollection<TrainingModuleLessonPlanTemplate> LessonPlanTemplates { get; set; } = [];
    public ICollection<TrainingModuleLessonPlan> LessonPlans { get; set; } = [];
    public ICollection<TrainingModuleEvaluation> GivenEvaluations { get; set; } = [];
}
