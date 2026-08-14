namespace DSC.Domain.Entities;

/// <summary>
/// Rúbrica o criterio de evaluación dinámica aplicable a los formadores.
/// </summary>
public class EvaluationCriterion
{
    public short Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public short? EvaluatorTypeId { get; set; }
    public decimal MaxScore { get; set; } = 100m;
    public bool IsActive { get; set; } = true;

    // Navegación
    public EvaluatorType? EvaluatorType { get; set; }
    public ICollection<TrainingModuleEvaluationDetail> EvaluationDetails { get; set; } = [];
}
