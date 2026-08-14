namespace DSC.Domain.Entities;

/// <summary>
/// Cargo orgánico o rol funcional en el movimiento scout (Jefe de Grupo, Responsable de Rama, Comisionado, etc.).
/// </summary>
public class ScoutPosition
{
    public short Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Colecciones de navegación
    public ICollection<ScoutAffiliation> Affiliations { get; set; } = [];
}
