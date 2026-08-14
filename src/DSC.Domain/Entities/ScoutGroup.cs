namespace DSC.Domain.Entities;

/// <summary>
/// Representa un Grupo Scout (organización perteneciente al distrito).
/// </summary>
public class ScoutGroup
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly? FoundationDate { get; set; }
    public long? FacilityId { get; set; }
    public bool IsActive { get; set; } = true;

    // Navegación
    public Facility? Facility { get; set; }
    public ICollection<ScoutGroupContactMechanism> ContactMechanisms { get; set; } = [];
    public ICollection<ScoutAffiliation> Affiliations { get; set; } = [];
}
