namespace DSC.Domain.Entities;

/// <summary>
/// Cronograma desglosado de actividades en minutos para la sesión dictada.
/// </summary>
public class TrainingModuleLessonPlanActivity
{
    public long Id { get; set; }
    public long LessonPlanId { get; set; }
    public short StepNumber { get; set; } = 1;
    public short DurationMinutes { get; set; }
    public string ActivityName { get; set; } = string.Empty;
    public string? Materials { get; set; }

    // Navegación
    public TrainingModuleLessonPlan LessonPlan { get; set; } = null!;
}
