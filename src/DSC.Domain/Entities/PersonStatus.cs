namespace DSC.Domain.Entities;

/// <summary>
/// Clasificador de estados de la persona en el registro distrital (ACTIVE, INACTIVE).
/// </summary>
public class PersonStatus
{
    public short Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Colecciones de navegación
    public ICollection<Person> Persons { get; set; } = [];
}
