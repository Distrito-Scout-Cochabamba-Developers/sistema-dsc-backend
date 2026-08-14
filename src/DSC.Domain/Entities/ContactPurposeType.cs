namespace DSC.Domain.Entities;

/// <summary>
/// Clasificador del propósito o contexto de uso del contacto (Personal, Institucional, Emergencia, etc.).
/// </summary>
public class ContactPurposeType
{
    public short Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Colecciones de navegación
    public ICollection<PersonContactMechanism> PersonContactMechanisms { get; set; } = [];
    public ICollection<ScoutGroupContactMechanism> ScoutGroupContactMechanisms { get; set; } = [];
}
