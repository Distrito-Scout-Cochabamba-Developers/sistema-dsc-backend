namespace DSC.Domain.Entities;

/// <summary>
/// Mecanismo de contacto institucional del Grupo Scout (correo institucional, teléfono de sede, etc.).
/// </summary>
public class ScoutGroupContactMechanism
{
    public long Id { get; set; }
    public long ScoutGroupId { get; set; }
    public short ContactMechanismTypeId { get; set; }
    public short ContactPurposeTypeId { get; set; }
    public string ContactValue { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }

    // Navegación
    public ScoutGroup ScoutGroup { get; set; } = null!;
    public ContactMechanismType ContactMechanismType { get; set; } = null!;
    public ContactPurposeType ContactPurposeType { get; set; } = null!;
}
