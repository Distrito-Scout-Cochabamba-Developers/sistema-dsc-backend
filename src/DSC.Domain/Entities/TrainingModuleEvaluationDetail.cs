namespace DSC.Domain.Entities;

/// <summary>
/// Puntuación individual y comentario asignado a un criterio de evaluación específico.
/// </summary>
public class TrainingModuleEvaluationDetail
{
    public long Id { get; set; }
    public long TrainingModuleEvaluationId { get; set; }
    public short CriterionId { get; set; }
    public decimal Score { get; set; }
    public string? Comment { get; set; }

    // Navegación
    public TrainingModuleEvaluation Evaluation { get; set; } = null!;
    public EvaluationCriterion Criterion { get; set; } = null!;
}
