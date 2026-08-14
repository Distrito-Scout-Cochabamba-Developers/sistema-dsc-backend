namespace DSC.Domain.Entities;

/// <summary>
/// Clasificador del rol de quien evalúa (PARTICIPANT, MANAGEMENT_TEAM).
/// </summary>
public class EvaluatorType
{
    public short Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Colecciones de navegación
    public ICollection<EvaluationCriterion> Criteria { get; set; } = [];
    public ICollection<TrainingModuleEvaluation> Evaluations { get; set; } = [];
}
