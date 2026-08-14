namespace DSC.Domain.Entities;

/// <summary>
/// Representa una instalación física, sede o ambiente del distrito scout o de un grupo scout.
/// Soporta jerarquías físicas (ej. Salón Principal dentro de la Casa Distrital).
/// </summary>
public class Facility
{
    public long Id { get; set; }
    public short FacilityTypeId { get; set; }
    public long? ParentFacilityId { get; set; }
    public string? Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PhysicalAddress { get; set; }
    public short? Capacity { get; set; }
    public bool IsActive { get; set; } = true;

    // Navegación
    public FacilityType FacilityType { get; set; } = null!;
    public Facility? ParentFacility { get; set; }
    public ICollection<Facility> ChildFacilities { get; set; } = [];
    public ICollection<ScoutGroup> ScoutGroups { get; set; } = [];
    public ICollection<TrainingModuleSchedule> Schedules { get; set; } = [];
}
