namespace DSC.Domain.Entities;

/// <summary>
/// Plantilla didáctica maestra oficial del módulo a nivel curricular distrital.
/// </summary>
public class TrainingModuleLessonPlanTemplate
{
    public long Id { get; set; }
    public long TrainingModuleId { get; set; }
    public long? AuthorPersonId { get; set; }
    public string VersionCode { get; set; } = "1.0";
    public string? GeneralObjective { get; set; }
    public string? SpecificObjectives { get; set; }
    public string? BibliographyReferences { get; set; }
    public long? SlideFileId { get; set; }
    public long? DocumentFileId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navegación
    public TrainingModule TrainingModule { get; set; } = null!;
    public Person? AuthorPerson { get; set; }
    public FileAsset? SlideFile { get; set; }
    public FileAsset? DocumentFile { get; set; }
    public ICollection<TrainingModuleLessonPlanTemplateActivity> Activities { get; set; } = [];
    public ICollection<TrainingModuleLessonPlan> DerivedLessonPlans { get; set; } = [];
}
