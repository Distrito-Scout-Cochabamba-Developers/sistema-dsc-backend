namespace DSC.Domain.Entities;

/// <summary>
/// Mecanismo de contacto de una persona con propósito explícito (personal, institucional, emergencias) y vigencia temporal.
/// </summary>
public class PersonContactMechanism
{
    public long Id { get; set; }
    public long PersonId { get; set; }
    public short ContactMechanismTypeId { get; set; }
    public short ContactPurposeTypeId { get; set; }
    public string ContactValue { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public DateOnly ValidFrom { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public DateOnly? ValidUntil { get; set; }

    // Navegación
    public Person Person { get; set; } = null!;
    public ContactMechanismType ContactMechanismType { get; set; } = null!;
    public ContactPurposeType ContactPurposeType { get; set; } = null!;
}
