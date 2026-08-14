namespace DSC.Domain.Entities;

/// <summary>
/// Catálogo general de módulos de formación curricular estándar.
/// </summary>
public class TrainingModule
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public short ModuleTypeId { get; set; }

    // Navegación
    public ModuleType ModuleType { get; set; } = null!;
    public ICollection<TrainingModuleSession> Sessions { get; set; } = [];
    public ICollection<TrainingModuleLessonPlanTemplate> LessonPlanTemplates { get; set; } = [];
}
