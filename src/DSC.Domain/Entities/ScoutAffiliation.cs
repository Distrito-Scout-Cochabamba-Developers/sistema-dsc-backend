namespace DSC.Domain.Entities;

/// <summary>
/// Representa la relación temporal de pertenencia de una persona a un Grupo Scout, Rama y Cargo (Party Relationship).
/// </summary>
public class ScoutAffiliation
{
    public long Id { get; set; }
    public long PersonId { get; set; }
    public long ScoutGroupId { get; set; }
    public short? BranchId { get; set; }
    public short? ScoutPositionId { get; set; }
    public DateOnly ValidFrom { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public DateOnly? ValidUntil { get; set; }
    public bool IsPrimary { get; set; } = true;

    // Navegación
    public Person Person { get; set; } = null!;
    public ScoutGroup ScoutGroup { get; set; } = null!;
    public Branch? Branch { get; set; }
    public ScoutPosition? ScoutPosition { get; set; }
}
