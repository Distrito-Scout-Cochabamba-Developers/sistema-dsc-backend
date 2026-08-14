namespace DSC.Domain.Entities;

/// <summary>
/// Desglose sugerido de actividades por minutos en la plantilla curricular maestra.
/// </summary>
public class TrainingModuleLessonPlanTemplateActivity
{
    public long Id { get; set; }
    public long TemplateId { get; set; }
    public short StepNumber { get; set; } = 1;
    public short DurationMinutes { get; set; }
    public string ActivityName { get; set; } = string.Empty;
    public string? RecommendedMaterials { get; set; }

    // Navegación
    public TrainingModuleLessonPlanTemplate Template { get; set; } = null!;
}
