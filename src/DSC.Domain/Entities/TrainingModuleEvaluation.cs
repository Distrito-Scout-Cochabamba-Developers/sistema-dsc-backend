namespace DSC.Domain.Entities;

/// <summary>
/// Cabecera de la evaluación realizada a un formador/disertante de un módulo de formación.
/// </summary>
public class TrainingModuleEvaluation
{
    public long Id { get; set; }
    public long TrainingModuleInstructorId { get; set; }
    public short EvaluatorTypeId { get; set; }
    public long? TrainingModuleEnrollmentId { get; set; }
    public long? EvaluatorPersonId { get; set; }
    public string? GeneralFeedback { get; set; }
    public decimal? AverageScore { get; set; }

    // Navegación
    public TrainingModuleInstructor Instructor { get; set; } = null!;
    public EvaluatorType EvaluatorType { get; set; } = null!;
    public TrainingModuleEnrollment? Enrollment { get; set; }
    public Person? EvaluatorPerson { get; set; }
    public ICollection<TrainingModuleEvaluationDetail> EvaluationDetails { get; set; } = [];
}
