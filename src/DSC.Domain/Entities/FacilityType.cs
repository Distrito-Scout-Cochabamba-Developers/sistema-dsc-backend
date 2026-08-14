namespace DSC.Domain.Entities;

/// <summary>
/// Clasificador del tipo de instalación o sede (Casa Distrital, Salón, Aula, Sede de Grupo, Campo Escuela).
/// </summary>
public class FacilityType
{
    public short Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Colecciones de navegación
    public ICollection<Facility> Facilities { get; set; } = [];
}
