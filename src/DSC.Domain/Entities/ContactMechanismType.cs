namespace DSC.Domain.Entities;

/// <summary>
/// Clasificador de tipos de mecanismo de contacto (Email, Celular, Teléfono Fijo, Dirección Física, etc.).
/// </summary>
public class ContactMechanismType
{
    public short Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Colecciones de navegación
    public ICollection<PersonContactMechanism> PersonContactMechanisms { get; set; } = [];
    public ICollection<ScoutGroupContactMechanism> ScoutGroupContactMechanisms { get; set; } = [];
}
