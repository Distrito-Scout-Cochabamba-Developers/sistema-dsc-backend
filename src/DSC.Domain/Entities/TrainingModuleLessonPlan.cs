namespace DSC.Domain.Entities;

/// <summary>
/// Plan didáctico específico preparado por el disertante para una sesión concreta (1:1 con TrainingModuleSession).
/// Opcionalmente derivado de una plantilla maestra distrital.
/// </summary>
public class TrainingModuleLessonPlan
{
    public long Id { get; set; }
    public long TrainingModuleSessionId { get; set; }
    public long? DerivedFromTemplateId { get; set; }
    public long? AuthorPersonId { get; set; }
    public string? GeneralObjective { get; set; }
    public string? SpecificObjectives { get; set; }
    public string? BibliographyReferences { get; set; }
    public long? SlideFileId { get; set; }
    public long? DocumentFileId { get; set; }
    public bool IsSubmitted { get; set; }
    public DateTime? SubmittedAt { get; set; }

    // Navegación
    public TrainingModuleSession Session { get; set; } = null!;
    public TrainingModuleLessonPlanTemplate? DerivedFromTemplate { get; set; }
    public Person? AuthorPerson { get; set; }
    public FileAsset? SlideFile { get; set; }
    public FileAsset? DocumentFile { get; set; }
    public ICollection<TrainingModuleLessonPlanActivity> Activities { get; set; } = [];
}
